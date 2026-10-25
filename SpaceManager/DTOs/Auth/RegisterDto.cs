using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Auth;

/// <summary>
/// Payload used to register a new individual member account.
/// </summary>
/// <param name="FirstName">The user's given name.</param>
/// <param name="LastName">The user's family name.</param>
/// <param name="Email">The unique email address used as username and communication channel.</param>
/// <param name="Password">The account password meeting complexity requirements.</param>
public record RegisterDto(
    [Required, MaxLength(50)] string FirstName,
    [Required, MaxLength(50)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);
