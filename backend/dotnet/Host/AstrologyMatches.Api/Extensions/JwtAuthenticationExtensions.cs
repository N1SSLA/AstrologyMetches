using System.Text;
using AstrologyMatches.Shared.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AstrologyMatches.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Подключает проверку JWT Bearer по настройкам из секции «Jwt».
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        if (jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Не задан ключ Jwt:Key (минимум 32 символа). Задайте его через dotnet user-secrets или переменную окружения Jwt__Key.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true
                };
            });

        services.AddAuthorization();

        return services;
    }
}
