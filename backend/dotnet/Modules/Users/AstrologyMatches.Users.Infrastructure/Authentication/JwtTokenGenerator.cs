using System.Security.Claims;
using System.Text;
using AstrologyMatches.Shared.Authentication;
using AstrologyMatches.Users.Application.Authentication;
using AstrologyMatches.Users.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AstrologyMatches.Users.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtToken Generate(User user)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimsPrincipalExtensions.UserIdClaimType, user.Id.ToString())
            }),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        string token = new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        return new JwtToken(token, expiresAt);
    }
}
