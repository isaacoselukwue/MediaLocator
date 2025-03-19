using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ML.Web.Models;

namespace ML.Web.Services;

public interface IMediaLocatorHttpClient
{
    Task<T> GetAsync<T>(string endpoint);
    Task<T> PostAsync<T, TRequest>(string endpoint, TRequest request);
    Task<T> PutAsync<T, TRequest>(string endpoint, TRequest request);
    Task<T> DeleteAsync<T>(string endpoint);
    Task<T> DeleteAsync<T, TRequest>(string endpoint, TRequest request);
    Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request);
    
    Task PostAsync<TRequest>(string endpoint, TRequest request);
    Task PutAsync<TRequest>(string endpoint, TRequest request);
}

public class MediaLocatorHttpClient : IMediaLocatorHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public MediaLocatorHttpClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        HttpResponseMessage response = await SendRequestWithAuthAsync(request);
        
        await EnsureSuccessResponseAsync(response);
        
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions) 
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<T> PostAsync<T, TRequest>(string endpoint, TRequest request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await SendRequestWithAuthAsync(httpRequest);
        
        await EnsureSuccessResponseAsync(response);
        
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await SendRequestWithAuthAsync(httpRequest);
        
        await EnsureSuccessResponseAsync(response);
    }

    public async Task<T> PutAsync<T, TRequest>(string endpoint, TRequest request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Put, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await SendRequestWithAuthAsync(httpRequest);
        
        await EnsureSuccessResponseAsync(response);
        
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }
    
    public async Task PutAsync<TRequest>(string endpoint, TRequest request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Put, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await SendRequestWithAuthAsync(httpRequest);
        
        await EnsureSuccessResponseAsync(response);
    }

    public async Task<T> DeleteAsync<T>(string endpoint)
    {
        HttpRequestMessage request = new(HttpMethod.Delete, endpoint);
        HttpResponseMessage response = await SendRequestWithAuthAsync(request);
        
        await EnsureSuccessResponseAsync(response);
        
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<T> DeleteAsync<T, TRequest>(string endpoint, TRequest request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Delete, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await SendRequestWithAuthAsync(httpRequest);

        await EnsureSuccessResponseAsync(response);

        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deseralise response");
    }

    public async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request)
    {
        return await SendRequestWithAuthAsync(request);
    }

    private async Task<HttpResponseMessage> SendRequestWithAuthAsync(HttpRequestMessage request)
    {
        //if (!IsAuthEndpoint(request.RequestUri?.AbsolutePath ?? string.Empty))
        //{
        //    var token = await _localStorage.GetItemAsync<string>("accessToken");
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    }
        //}
        
        return await _httpClient.SendAsync(request);
    }

    //private static bool IsAuthEndpoint(string path)
    //{
    //    return _authEndpoints.Any(endpoint => path.Contains(endpoint, StringComparison.OrdinalIgnoreCase));
    //}
    
    private async Task EnsureSuccessResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            
            try
            {
                ApiResult? errorResponse = JsonSerializer.Deserialize<ApiResult>(errorContent, _jsonOptions);
                
                if (errorResponse is not null)
                {
                    var exception = new HttpRequestException($"API returned {response.StatusCode}");
                    exception.Data["ApiErrorResponse"] = new ErrorResponse 
                    { 
                        Message = errorResponse.Message ?? "API request failed",
                        Errors = errorResponse.Errors
                    };
                    throw exception;
                }
            }
            catch (JsonException)
            {

            }
            
            throw new HttpRequestException($"API request failed with status code: {response.StatusCode}", null, response.StatusCode);
        }
    }
}