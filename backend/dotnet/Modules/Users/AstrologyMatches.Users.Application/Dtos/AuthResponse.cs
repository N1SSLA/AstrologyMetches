namespace AstrologyMatches.Users.Application.Dtos;

/// <summary>
/// Ответ на регистрацию и вход: JWT для заголовка Authorization и срок его действия (UTC).
/// </summary>
public record AuthResponse(Guid UserId, string AccessToken, DateTime ExpiresAt);
