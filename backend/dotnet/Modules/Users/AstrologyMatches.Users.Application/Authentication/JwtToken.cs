namespace AstrologyMatches.Users.Application.Authentication;

/// <summary>
/// Выпущенный JWT и момент (UTC), когда он перестанет действовать.
/// </summary>
public record JwtToken(string Value, DateTime ExpiresAt);
