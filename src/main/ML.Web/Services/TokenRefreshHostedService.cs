using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ML.Web.Services;

public class TokenRefreshHostedService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoRefreshCheck, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }
    
    private async void DoRefreshCheck(object? state)
    {
        using var scope = _serviceProvider.CreateScope();
        ITokenRefreshService tokenService = scope.ServiceProvider.GetRequiredService<ITokenRefreshService>();
        
        if (await tokenService.IsTokenAboutToExpireAsync(10))
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            await authService.RefreshToken();
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}