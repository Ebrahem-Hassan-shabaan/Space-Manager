using SpaceManager.DTOs.Auth;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines authentication and user registration service operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="registerDto">The registration data transfer object.</param>
    /// <returns>An authentication response DTO containing token or error messages.</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Authenticates a user and generates a JWT access token.
    /// </summary>
    /// <param name="loginDto">The login data transfer object.</param>
    /// <returns>An authentication response DTO containing token or error messages.</returns>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}
