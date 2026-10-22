namespace AstrologyMatches.Shared.Exceptions;

/// <summary>
/// Нарушено бизнес-правило или данные запроса некорректны. Отдаётся клиенту как 400.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
