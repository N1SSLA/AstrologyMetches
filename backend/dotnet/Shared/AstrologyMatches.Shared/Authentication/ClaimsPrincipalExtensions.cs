using System.Security.Claims;

namespace AstrologyMatches.Shared.Authentication;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Claim с Id пользователя в JWT (стандартный «sub»).
    /// </summary>
    public const string UserIdClaimType = "sub";

    /// <summary>
    /// Возвращает Id текущего пользователя из JWT. Вызывать только в действиях с [Authorize].
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? value = principal.FindFirst(UserIdClaimType)?.Value;
        if (!Guid.TryParse(value, out Guid userId))
        {
            throw new InvalidOperationException("В токене нет корректного Id пользователя.");
        }

        return userId;
    }
}
