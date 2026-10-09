using Microsoft.AspNetCore.Mvc;
using SpaceManager.DTOs.Auth;
using SpaceManager.Services.Interfaces;
namespace SpaceManager.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AuthController:ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) { 
        _authService = authService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);


    }





    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    

}
