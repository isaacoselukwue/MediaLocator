using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;

namespace ML.Web.Services;

public class NavigationAuthorizationHandler : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public NavigationAuthorizationHandler(
        NavigationManager navigationManager,
        ITokenRefreshService tokenRefreshService,
        AuthenticationStateProvider authStateProvider)
    {
        _navigationManager = navigationManager;
        _tokenRefreshService = tokenRefreshService;
        _authStateProvider = authStateProvider;

        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        string path = new Uri(e.Location).PathAndQuery;
        if (path.StartsWith("/login") || path.StartsWith("/logout") || path.StartsWith("/auth"))
            return;

        await _tokenRefreshService.CheckAndRefreshTokenAsync();
        
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (path.StartsWith("/media", StringComparison.OrdinalIgnoreCase))
        {
            if (!user.Identity?.IsAuthenticated ?? true || !user.IsInRole("User"))
            {
                _navigationManager.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString(path));
                return;
            }
        }
        
        if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!user.Identity?.IsAuthenticated ?? true || !user.IsInRole("Admin"))
            {
                _navigationManager.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString(path));
                return;
            }
        }
        
        if (path.StartsWith("/account", StringComparison.OrdinalIgnoreCase))
        {
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _navigationManager.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString(path));
                return;
            }
        }
        
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }
}