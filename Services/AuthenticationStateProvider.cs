using System.Security.Claims;
using BlazorApp4.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp4.Services;

public class CustomAuthProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new();

    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _currentUser.Identity is not null && _currentUser.Identity.IsAuthenticated
            ? new ClaimsIdentity(_currentUser.Identity as ClaimsIdentity)
            : new ClaimsIdentity();

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public Task SetAuthAsync(User user)
    {
        CurrentUser = user;
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        }, "cookie"));

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        _currentUser = new ClaimsPrincipal();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }
}