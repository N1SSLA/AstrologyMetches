namespace AstrologyMatches.Shared.Exceptions;

/// <summary>
/// Действие конфликтует с текущим состоянием данных (например, email уже занят). Отдаётся клиенту как 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
