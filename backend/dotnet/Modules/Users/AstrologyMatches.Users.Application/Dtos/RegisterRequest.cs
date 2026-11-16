using System.ComponentModel.DataAnnotations;

namespace AstrologyMatches.Users.Application.Dtos;

/// <summary>
/// Запрос на регистрацию по email и паролю.
/// </summary>
public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);
