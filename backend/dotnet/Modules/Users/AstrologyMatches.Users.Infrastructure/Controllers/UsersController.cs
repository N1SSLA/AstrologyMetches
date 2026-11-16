using AstrologyMatches.Shared.Authentication;
using AstrologyMatches.Users.Application.Dtos;
using AstrologyMatches.Users.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AstrologyMatches.Users.Infrastructure.Controllers;

/// <summary>
/// Регистрация, вход и управление своим аккаунтом.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Регистрирует пользователя по email и паролю и сразу выдаёт JWT.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        AuthResponse response = await _userService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMeAsync), response);
    }

    /// <summary>
    /// Вход по email и паролю. Возвращает JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponse response = await _userService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Данные аккаунта текущего пользователя.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetMeAsync(CancellationToken cancellationToken)
    {
        UserDto user = await _userService.GetByIdAsync(User.GetUserId(), cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Удаляет аккаунт текущего пользователя.
    /// </summary>
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMeAsync(CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
