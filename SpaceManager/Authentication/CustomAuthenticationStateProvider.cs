using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using SpaceManager.Models;

namespace SpaceManager.Authentication;

public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        UserManager<ApplicationUser> userManager)
    {
        _sessionStorage = sessionStorage;
        _userManager = userManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionResult = await _sessionStorage.GetAsync<UserSession>("SpaceManager_UserSession");
            var userSession = userSessionResult.Success ? userSessionResult.Value : null;

            if (userSession == null || string.IsNullOrWhiteSpace(userSession.UserId))
            {
                return new AuthenticationState(_anonymous);
            }

            var user = await _userManager.FindByIdAsync(userSession.UserId);
            if (user == null)
            {
                return new AuthenticationState(_anonymous);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? user.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "SpaceManagerAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // During prerendering before JS interop is active or on storage error, return anonymous
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticatedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        var session = new UserSession
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty
        };

        await _sessionStorage.SetAsync("SpaceManager_UserSession", session);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "SpaceManagerAuth");
        var userPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync("SpaceManager_UserSession");
        }
        catch
        {
            // Ignore storage deletion errors
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
