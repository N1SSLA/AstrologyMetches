namespace AstrologyMatches.Shared.Exceptions;

/// <summary>
/// У пользователя нет прав на действие (например, чужой профиль). Отдаётся клиенту как 403.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
