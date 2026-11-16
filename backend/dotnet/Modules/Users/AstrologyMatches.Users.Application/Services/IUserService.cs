using AstrologyMatches.Users.Application.Dtos;

namespace AstrologyMatches.Users.Application.Services;

/// <summary>
/// Регистрация, вход и управление аккаунтом.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Создаёт аккаунт и сразу выдаёт JWT. Если email занят, бросает ConflictException.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Проверяет email и пароль и выдаёт JWT. При неверных данных бросает ValidationException.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Помечает аккаунт удалённым. Войти в него после этого нельзя.
    /// </summary>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}
