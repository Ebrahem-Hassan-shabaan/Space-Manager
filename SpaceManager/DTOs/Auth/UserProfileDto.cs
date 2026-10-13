namespace SpaceManager.DTOs.Auth;

/// <summary>
/// Data transfer object representing the authenticated user's profile.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="FullName">The full display name of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="RegisteredAt">The UTC date and time when the user registered.</param>
public record UserProfileDto(
    string Id,
    string FullName,
    string Email,
    DateTime RegisteredAt
);
