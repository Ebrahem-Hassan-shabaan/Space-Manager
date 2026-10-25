namespace SpaceManager.DTOs.Auth;

/// <summary>
/// Data transfer object returned upon authentication attempts (login, registration).
/// </summary>
/// <param name="Success">Indicates whether the authentication operation succeeded.</param>
/// <param name="Message">Descriptive status message or error feedback.</param>
/// <param name="Token">JWT bearer authentication token if successful.</param>
public record AuthResponseDto(bool Success, string Message, string Token = "");
