using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Auth;
using SpaceManager.Services.Interfaces;

namespace SpaceManager.Controllers;

/// <summary>
/// Provides API endpoints for user authentication and registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service instance.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration request details.</param>
    /// <returns>An authentication response DTO containing JWT token or error message.</returns>
    /// <response code="200">User successfully registered.</response>
    /// <response code="400">If registration failed (e.g. user already exists or validation errors).</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticates an existing user and issues a JWT token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>An authentication response DTO containing JWT token or error message.</returns>
    /// <response code="200">User successfully logged in.</response>
    /// <response code="400">If credentials are invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The authenticated user's profile details.</returns>
    /// <response code="200">Returns the user's profile.</response>
    /// <response code="401">If the user is unauthenticated.</response>
    /// <response code="404">If the user profile could not be found.</response>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null) return NotFound(new { message = "User profile not found." });

        return Ok(profile);
    }
}
