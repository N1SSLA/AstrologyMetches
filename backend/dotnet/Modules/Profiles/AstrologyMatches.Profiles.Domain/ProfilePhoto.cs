namespace AstrologyMatches.Profiles.Domain;

/// <summary>
/// Метаданные фотографии профиля. Сам файл лежит в хранилище изображений под ключом ObjectKey.
/// </summary>
public class ProfilePhoto
{
    public const int MaxPhotosPerProfile = 6;
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public Guid Id { get; private set; }

    public Guid ProfileId { get; private set; }

    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Порядок в карусели, начиная с 0.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Главное фото показывается в карточке кандидата и в списке чатов.
    /// </summary>
    public bool IsMain { get; private set; }

    /// <summary>
    /// Ключ объекта в хранилище: {profileId}/{photoId}.
    /// </summary>
    public string ObjectKey => $"{ProfileId}/{Id}";

    // Нужен EF Core, чтобы создавать объект при чтении из БД
    private ProfilePhoto()
    {
    }

    public ProfilePhoto(Guid profileId, string contentType, int position, bool isMain)
    {
        Id = Guid.NewGuid();
        ProfileId = profileId;
        ContentType = contentType;
        Position = position;
        IsMain = isMain;
    }

    public void MakeMain()
    {
        IsMain = true;
    }

    public void RemoveMain()
    {
        IsMain = false;
    }

    public void MoveTo(int position)
    {
        Position = position;
    }
}
