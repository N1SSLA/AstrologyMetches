namespace AstrologyMatches.Shared.Authentication;

/// <summary>
/// Настройки JWT из секции «Jwt» конфигурации. Host по ним проверяет токены, модуль Users по ним же их выпускает.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ подписи, не короче 32 символов. Хранится в user-secrets или переменной окружения Jwt__Key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public int LifetimeMinutes { get; set; } = 60;
}
