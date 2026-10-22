using AstrologyMatches.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AstrologyMatches.Api.Middleware;

/// <summary>
/// Ловит исключения из контроллеров и сервисов и превращает их в ответ ProblemDetails с нужным статус-кодом.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode = GetStatusCode(exception);

        string detail;
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Необработанная ошибка при запросе {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // Текст неизвестной ошибки может раскрыть внутренности системы, поэтому наружу отдаём общий
            detail = "Внутренняя ошибка сервера.";
        }
        else
        {
            _logger.LogWarning("Запрос {Method} {Path} завершился с кодом {StatusCode}: {Message}",
                context.Request.Method, context.Request.Path, statusCode, exception.Message);

            detail = exception.Message;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound => "Не найдено",
            StatusCodes.Status400BadRequest => "Некорректный запрос",
            StatusCodes.Status403Forbidden => "Доступ запрещён",
            StatusCodes.Status409Conflict => "Конфликт данных",
            _ => "Ошибка сервера"
        };
    }
}
