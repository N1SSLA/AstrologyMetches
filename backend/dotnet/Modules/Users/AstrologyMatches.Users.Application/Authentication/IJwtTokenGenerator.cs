using AstrologyMatches.Users.Domain;

namespace AstrologyMatches.Users.Application.Authentication;

/// <summary>
/// Выпускает JWT для пользователя.
/// </summary>
public interface IJwtTokenGenerator
{
    JwtToken Generate(User user);
}
