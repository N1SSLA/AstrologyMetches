using System.ComponentModel.DataAnnotations;

namespace AstrologyMatches.Users.Application.Dtos;

/// <summary>
/// Запрос на вход по email и паролю.
/// </summary>
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
