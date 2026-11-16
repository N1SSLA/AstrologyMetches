using AstrologyMatches.Profiles.Application;
using AstrologyMatches.Profiles.Application.Services;
using AstrologyMatches.Profiles.Application.Storage;
using AstrologyMatches.Profiles.Infrastructure.Persistence;
using AstrologyMatches.Profiles.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace AstrologyMatches.Profiles.Infrastructure;

public static class ProfilesModule
{
    /// <summary>
    /// Регистрирует всё, что нужно модулю Profiles: DbContext, хранилище фото на MinIO, сервисы и маппинг.
    /// </summary>
    public static IServiceCollection AddProfilesModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:Database. Задайте её через dotnet user-secrets.");

        services.AddDbContext<ProfilesDbContext>(options => options
            // У каждого модуля своя таблица истории миграций в своей схеме, чтобы миграции модулей не мешали друг другу
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", ProfilesDbContext.SchemaName))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IProfilesDbContext>(provider => provider.GetRequiredService<ProfilesDbContext>());

        AddMinioPhotoStorage(services, configuration);

        services.AddScoped<IProfilePhotoService, ProfilePhotoService>();

        services.AddAutoMapper(config => config.AddProfile<ProfilesMappingProfile>());

        return services;
    }

    private static void AddMinioPhotoStorage(IServiceCollection services, IConfiguration configuration)
    {
        var minioSection = configuration.GetSection(MinioOptions.SectionName);
        services.Configure<MinioOptions>(minioSection);

        var minioOptions = minioSection.Get<MinioOptions>() ?? new MinioOptions();
        if (string.IsNullOrEmpty(minioOptions.Endpoint)
            || string.IsNullOrEmpty(minioOptions.AccessKey)
            || string.IsNullOrEmpty(minioOptions.SecretKey))
        {
            throw new InvalidOperationException(
                "Не заданы настройки Minio:Endpoint, Minio:AccessKey или Minio:SecretKey. Ключи задайте через dotnet user-secrets.");
        }

        // Клиент MinIO потокобезопасен, поэтому один на всё приложение
        services.AddSingleton<IMinioClient>(_ => new MinioClient()
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .WithSSL(minioOptions.UseSsl)
            .WithRegion(minioOptions.Region)
            .Build());

        services.AddScoped<IPhotoStorage, MinioPhotoStorage>();
    }
}
