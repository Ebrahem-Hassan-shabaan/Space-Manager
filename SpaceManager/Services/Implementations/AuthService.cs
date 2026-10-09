
using SpaceManager.Models;
using SpaceManager.Services.Interfaces;
using SpaceManager.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

namespace SpaceManager.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager,IConfiguration configuration)
    {
                _userManager = userManager;
                _configuration = configuration;

    }
    public async Task<AuthResponseDto> LoginAsync(LoginDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponseDto(false, "Invalid Credentials.");
        }
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto(false, "Invalid Credentials.");
        }
        var token =await GenerateJwtTokenAsync(user);
        return new AuthResponseDto(true,"Login Successful", token);





    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
    {
        var existingUser =await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto(false, "User already exists");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            FullName = $"{request.FirstName} {request.LastName}",
            UserName = request.Email

        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if(!result.Succeeded)
        {
            return new AuthResponseDto(false,result.Errors.FirstOrDefault()?.Description??"Registeration Failed.");
        }
        await _userManager.AddToRoleAsync(user, "User");
        var token = await GenerateJwtTokenAsync(user);
        return new AuthResponseDto(true,"User Registered Successfully.",token);


    }  
    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await  _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]??throw new InvalidOperationException("JWT Secret Key is not configured.")));
        var cred= new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
            signingCredentials: cred
        );
        return new JwtSecurityTokenHandler().WriteToken(token);

    }
}
