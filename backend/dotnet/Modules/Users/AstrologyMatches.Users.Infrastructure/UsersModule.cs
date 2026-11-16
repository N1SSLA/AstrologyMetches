using AstrologyMatches.Users.Application;
using AstrologyMatches.Users.Application.Authentication;
using AstrologyMatches.Users.Application.Services;
using AstrologyMatches.Users.Domain;
using AstrologyMatches.Users.Infrastructure.Authentication;
using AstrologyMatches.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AstrologyMatches.Users.Infrastructure;

public static class UsersModule
{
    /// <summary>
    /// Регистрирует всё, что нужно модулю Users: DbContext, сервисы, выпуск JWT и маппинг.
    /// </summary>
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:Database. Задайте её через dotnet user-secrets.");

        services.AddDbContext<UsersDbContext>(options => options
            // У каждого модуля своя таблица истории миграций в своей схеме, чтобы миграции модулей не мешали друг другу
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", UsersDbContext.SchemaName))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IUsersDbContext>(provider => provider.GetRequiredService<UsersDbContext>());

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserService, UserService>();

        services.AddAutoMapper(config => config.AddProfile<UsersMappingProfile>());

        return services;
    }
}
