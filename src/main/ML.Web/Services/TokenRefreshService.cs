namespace ML.Web.Services;

public interface ITokenRefreshService
{
    Task<bool> CheckAndRefreshTokenAsync();
    Task<bool> IsTokenExpiredAsync();
    Task<bool> IsTokenAboutToExpireAsync(int minutesThreshold = 5);
}

public class TokenRefreshService(
    ILocalStorageService localStorage,
    IAuthenticationService authService) : ITokenRefreshService
{
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly IAuthenticationService _authService = authService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _isRefreshing = false;

    public async Task<bool> CheckAndRefreshTokenAsync()
    {
        if (_isRefreshing)
            return false;

        if (!await IsTokenAboutToExpireAsync())
            return true;

        try
        {
            await _semaphore.WaitAsync();
            _isRefreshing = true;

            if (!await IsTokenAboutToExpireAsync())
                return true;

            return await _authService.RefreshToken();
        }
        finally
        {
            _isRefreshing = false;
            _semaphore.Release();
        }
    }

    public async Task<bool> IsTokenExpiredAsync()
    {
        DateTime expiryTime = await GetTokenExpiryTimeAsync();
        return expiryTime <= DateTime.UtcNow;
    }

    public async Task<bool> IsTokenAboutToExpireAsync(int minutesThreshold = 5)
    {
        DateTime expiryTime = await GetTokenExpiryTimeAsync();
        return expiryTime <= DateTime.UtcNow.AddMinutes(minutesThreshold);
    }

    private async Task<DateTime> GetTokenExpiryTimeAsync()
    {
        DateTime expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiryTime");
        
        if (expiry == default)
        {
            return DateTime.UtcNow.AddMinutes(-1);
        }
        
        return expiry;
    }
}