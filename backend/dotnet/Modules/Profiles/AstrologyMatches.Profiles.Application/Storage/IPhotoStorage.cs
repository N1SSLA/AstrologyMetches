namespace AstrologyMatches.Profiles.Application.Storage;

/// <summary>
/// Хранилище файлов фотографий. Реализация на MinIO лежит в Infrastructure, остальной код про MinIO не знает.
/// </summary>
public interface IPhotoStorage
{
    Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>
    /// Временная ссылка, по которой браузер скачивает фото напрямую из хранилища.
    /// </summary>
    Task<string> GetDownloadUrlAsync(string objectKey);
}
