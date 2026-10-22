using Microsoft.OpenApi;

namespace AstrologyMatches.Api.Extensions;

public static class SwaggerExtensions
{
    private const string BearerSchemeName = "Bearer";

    /// <summary>
    /// Подключает Swagger с XML-документацией всех сборок решения и кнопкой Authorize для JWT.
    /// </summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "AstrologyMatches API", Version = "v1" });

            // Каждый проект решения кладёт рядом со сборкой свой XML-файл, поэтому новые модули подхватятся сами
            foreach (string xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "AstrologyMatches.*.xml"))
            {
                options.IncludeXmlComments(xmlFile);
            }

            options.AddSecurityDefinition(BearerSchemeName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Вставьте JWT-токен, полученный при входе (без слова Bearer)."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = []
            });
        });

        return services;
    }
}
