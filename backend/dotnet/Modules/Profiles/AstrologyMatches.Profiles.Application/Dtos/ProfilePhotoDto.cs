namespace AstrologyMatches.Profiles.Application.Dtos;

/// <summary>
/// Фото профиля для клиента.
/// </summary>
public record ProfilePhotoDto
{
    public Guid Id { get; init; }

    /// <summary>
    /// Порядок в карусели, начиная с 0.
    /// </summary>
    public int Position { get; init; }

    public bool IsMain { get; init; }

    /// <summary>
    /// Временная ссылка на файл (presigned URL). Когда истечёт, нужно заново запросить список фото.
    /// </summary>
    public string Url { get; init; } = string.Empty;
}
