using AstrologyMatches.Shared.Exceptions;

namespace AstrologyMatches.Users.Domain;

/// <summary>
/// Учётная запись пользователя: email для входа и хеш пароля.
/// </summary>
public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    // Нужен EF Core, чтобы создавать объект при чтении из БД
    private User()
    {
    }

    public User(string email)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        Status = UserStatus.Active;
    }

    /// <summary>
    /// Приводит email к одному виду, чтобы «Ivan@Mail.ru » и «ivan@mail.ru» считались одним адресом.
    /// </summary>
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Помечает аккаунт удалённым и стирает email и хеш пароля.
    /// </summary>
    public void MarkDeleted()
    {
        if (Status == UserStatus.Deleted)
        {
            throw new ValidationException("Аккаунт уже удалён.");
        }

        Status = UserStatus.Deleted;

        // Строку не удаляем: на Id пользователя ссылаются другие модули.
        // Email заменяем заглушкой, чтобы не хранить персональные данные и чтобы адрес можно было зарегистрировать заново
        Email = $"deleted-{Id}@deleted.local";
        PasswordHash = string.Empty;
    }
}
