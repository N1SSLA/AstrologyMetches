namespace AstrologyMatches.Shared.Exceptions;

/// <summary>
/// Запрошенный объект не найден. Отдаётся клиенту как 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
