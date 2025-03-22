using Microsoft.JSInterop;

namespace ML.Web.Services;

public class UserActivityService(
    IJSRuntime jsRuntime,
    ITokenRefreshService tokenRefreshService) : IDisposable
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ITokenRefreshService _tokenRefreshService = tokenRefreshService;
    private DotNetObjectReference<UserActivityService>? _dotNetReference;

    public async Task InitializeHeartbeat()
    {
        try
        {
            if (_dotNetReference == null)
            {
                _dotNetReference = DotNetObjectReference.Create(this);
                await _jsRuntime.InvokeVoidAsync("setupTokenRefreshHeartbeat", _dotNetReference);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing heartbeat: {ex.Message}");
        }
    }
    
    [JSInvokable]
    public async Task RefreshTokenIfNeeded()
    {
        try
        {
            await _tokenRefreshService.CheckAndRefreshTokenAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error refreshing token: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        try
        {
            _jsRuntime.InvokeVoidAsync("clearTokenRefreshHeartbeat").AsTask().Wait();
            
            _dotNetReference?.Dispose();
            _dotNetReference = null;
        }
        catch
        {
        }
        GC.SuppressFinalize(this);
    }
}