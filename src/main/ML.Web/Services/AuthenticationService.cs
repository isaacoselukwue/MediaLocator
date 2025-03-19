using Microsoft.AspNetCore.Components.Authorization;
using ML.Web.Models;

namespace ML.Web.Services;

public interface IAuthenticationService
{
    Task<bool> Login(string email, string password);
    Task Logout();
    Task<bool> RefreshToken();
    Task<bool> SignUp(
        string emailAddress,
        string password,
        string confirmPassword,
        string firstName,
        string lastName,
        string phoneNumber);
}

public class AuthenticationService(
        IMediaLocatorHttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider) : IAuthenticationService
{
    private readonly IMediaLocatorHttpClient _httpClient = httpClient;
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<bool> Login(string email, string password)
    {
        LoginRequest loginRequest = new() { EmailAddress = email, Password = password };

        try
        {
            ApiResult<LoginResponse>? result = await _httpClient.PostAsync<ApiResult<LoginResponse>, LoginRequest>("api/v1/authentication/login", loginRequest);

            if (result.Succeeded && result.Data?.AccessToken != null)
            {
                await StoreTokensAsync(result.Data.AccessToken);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication();
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }

        return false;
    }

    private async Task StoreTokensAsync(AccessTokenResponse accessToken)
    {
        await _localStorage.SetItemAsync("accessToken", accessToken.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", accessToken.RefreshToken);
        await _localStorage.SetItemAsync("tokenExpiry", accessToken.ExpiresIn);
        var expiryTime = DateTime.UtcNow.AddSeconds(accessToken.ExpiresIn);
        await _localStorage.SetItemAsync("tokenExpiryTime", expiryTime);
    }

    public async Task Logout()
    {
        string? refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

        if (!string.IsNullOrEmpty(refreshToken))
        {
            RevokeTokenRequest logoutRequest = new() { EncryptedToken = refreshToken };
            await _httpClient.PostAsync("api/v1/authentication/logout", logoutRequest);
        }

        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("tokenExpiry");

        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    public async Task<bool> RefreshToken()
    {
        string? refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

        if (string.IsNullOrEmpty(refreshToken))
            return false;

        RefreshTokenRequest refreshRequest = new() { EncryptedToken = refreshToken };

        try
        {
            ApiResult<LoginResponse>? result = await _httpClient.PostAsync<ApiResult<LoginResponse>, RefreshTokenRequest>("api/v1/authentication/login/refresh", refreshRequest);

            if (result == null || !result.Succeeded || result.Data?.AccessToken == null)
                return false;

            await StoreTokensAsync(result.Data.AccessToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SignUp(string emailAddress, string password, string confirmPassword, string firstName, string lastName, string phoneNumber)
    {
        SignupRequest signupRequest = new()
        {
            EmailAddress = emailAddress,
            Password = password,
            ConfirmPassword = confirmPassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        };

        try
        {
            ApiResult result = await _httpClient.PostAsync<ApiResult, SignupRequest>("api/v1/authentication/signup", signupRequest);

            return result.Succeeded;
        }
        catch (Exception)
        {
            throw;
        }
    }
}