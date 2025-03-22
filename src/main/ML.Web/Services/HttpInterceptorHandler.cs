using System.Net;
using System.Net.Http.Headers;

namespace ML.Web.Services;

public class HttpInterceptorHandler(ILocalStorageService localStorage, IServiceProvider serviceProvider) : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly string[] _authEndpoints = ["login", "refresh", "logout", "signup", "verify"];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!IsAuthEndpoint(request.RequestUri?.AbsolutePath ?? string.Empty))
        {
            var tokenRefreshService = _serviceProvider.GetService<ITokenRefreshService>();
    
            if (tokenRefreshService != null)
            {
                bool tokenValid = await tokenRefreshService.CheckAndRefreshTokenAsync();
                if (tokenValid)
                {
                    var token = await _localStorage.GetItemAsync<string>("accessToken");
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
            }
            else
            {
                var token = await _localStorage.GetItemAsync<string>("accessToken");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            !IsAuthEndpoint(request.RequestUri?.AbsolutePath ?? string.Empty))
        {
            var tokenRefreshService = _serviceProvider.GetService<ITokenRefreshService>();
            if (tokenRefreshService != null)
            {
                bool refreshed = await tokenRefreshService.CheckAndRefreshTokenAsync();
                if (refreshed)
                {
                    var newRequest = await CloneHttpRequestMessageAsync(request);
                    
                    var token = await _localStorage.GetItemAsync<string>("accessToken");
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    
                    return await base.SendAsync(newRequest, cancellationToken);
                }
            }
        }
        
        return response;
    }

    private static bool IsAuthEndpoint(string path)
    {
        return _authEndpoints.Any(endpoint => path.Contains(endpoint, StringComparison.OrdinalIgnoreCase));
    }
    
    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        HttpRequestMessage clone = new(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            clone.Content = new StringContent(content, System.Text.Encoding.UTF8, request.Content.Headers.ContentType?.MediaType);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}