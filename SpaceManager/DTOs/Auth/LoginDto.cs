using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Auth;

/// <summary>
/// Credentials payload used to sign in to the platform.
/// </summary>
/// <param name="Email">The user's registered account email address.</param>
/// <param name="Password">The account password.</param>
public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
