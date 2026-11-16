namespace AstrologyMatches.Profiles.Infrastructure.Storage;

/// <summary>
/// Настройки MinIO из секции «Minio» конфигурации.
/// </summary>
public class MinioOptions
{
    public const string SectionName = "Minio";

    /// <summary>
    /// Адрес MinIO без схемы, например localhost:9000. По этому же адресу браузер откроет presigned URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Логин MinIO. Хранится в user-secrets.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Пароль MinIO. Хранится в user-secrets.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = "profile-photos";

    public bool UseSsl { get; set; }

    /// <summary>
    /// Регион по умолчанию у MinIO. Если его не указать, клиент сходит в MinIO узнать регион при каждой подписи ссылки.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    public int UrlLifetimeMinutes { get; set; } = 60;
}
