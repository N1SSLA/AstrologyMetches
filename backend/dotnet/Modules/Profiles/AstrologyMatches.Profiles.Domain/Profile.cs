namespace AstrologyMatches.Profiles.Domain;

/// <summary>
/// Анкета пользователя. Пока только связь с аккаунтом: поля анкеты (имя, пол, город и т.д.) добавит автор модуля Profiles.
/// </summary>
public class Profile
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Id аккаунта из модуля Users.
    /// </summary>
    public Guid UserId { get; private set; }

    // Нужен EF Core, чтобы создавать объект при чтении из БД
    private Profile()
    {
    }

    public Profile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }
}
