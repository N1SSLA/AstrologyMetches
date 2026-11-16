using AstrologyMatches.Profiles.Application.Dtos;

namespace AstrologyMatches.Profiles.Application.Services;

/// <summary>
/// Загрузка, удаление и просмотр фотографий профиля.
/// </summary>
public interface IProfilePhotoService
{
    Task<IReadOnlyList<ProfilePhotoDto>> GetMyPhotosAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Фото чужого профиля по его Id (для карточки кандидата и профиля собеседника).
    /// </summary>
    Task<IReadOnlyList<ProfilePhotoDto>> GetPhotosAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// Проверяет размер и формат файла, кладёт его в хранилище и сохраняет метаданные.
    /// Поток должен поддерживать Seek: формат читается из первых байт, потом файл загружается целиком.
    /// </summary>
    Task<ProfilePhotoDto> UploadAsync(Guid userId, Stream content, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid photoId, CancellationToken cancellationToken);

    Task SetMainAsync(Guid userId, Guid photoId, CancellationToken cancellationToken);
}
