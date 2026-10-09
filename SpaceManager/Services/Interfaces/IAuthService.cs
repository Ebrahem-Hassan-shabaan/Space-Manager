using SpaceManager.DTOs.Auth;
namespace SpaceManager.Services.Interfaces;

public interface IAuthService
{
    Task <AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task <AuthResponseDto> LoginAsync(LoginDto loginDto);
}
