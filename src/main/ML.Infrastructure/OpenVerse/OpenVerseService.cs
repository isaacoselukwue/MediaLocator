using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ML.Infrastructure.OpenVerse.DTOs;
using System.Text.Json;
using RequestException = ML.Application.Common.Exceptions.HttpRequestException;

namespace ML.Infrastructure.OpenVerse;
internal class OpenVerseService(IHttpClientFactory httpClientFactory, ILogger<OpenVerseService> logger, HybridCache hybridCache, IOptions<OpenVerseSettings> openVerseSettings) 
    : IOpenVerseService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly OpenVerseSettings openVerseSettings = openVerseSettings.Value;

    private HttpClient InitialiseClient()
    {
        return _httpClientFactory.CreateClient("OpenVerseClient");
    }
    public async Task<string> GetAuthToken()
    {
        HttpClient client = InitialiseClient();
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "/v1/auth_tokens/token/");
        MultipartFormDataContent content = new()
        {
            { new StringContent(openVerseSettings.ClientId!), "client_id" },
            { new StringContent(openVerseSettings.ClientSecret!), "client_secret" },
            { new StringContent(openVerseSettings.GrantType!), "grant_type" }
        };
        requestMessage.Content = content;
        HttpResponseMessage response = await client.SendAsync(requestMessage);

        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get auth token: {responseContent}, {statusCode}, {reasonPhrase}", responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get auth token");
        }
        TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, GetJsonSerializerOptions())!;
        await hybridCache.SetAsync(nameof(OpenVerseSettings), tokenResponse.AccessToken, new HybridCacheEntryOptions { Expiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn), LocalCacheExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn) });
        return tokenResponse.AccessToken!;
    }
    public async Task<RateLimitResponse> GetRateLimitAsync()
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync("/v1/rate_limit");
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get rate limit: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get rate limit");
        }
        RateLimitResponse rateLimit = JsonSerializer.Deserialize<RateLimitResponse>(responseContent, GetJsonSerializerOptions())!;
        return rateLimit;
    }
    public async Task<AudioSearchResponse> SearchAudioAsync(
        string query,
        string license = "",
        string categories = "",
        int pageSize = 20,
        int page = 1)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var requestUrl = "/v1/audio/?q=" + query;
        if (!string.IsNullOrWhiteSpace(license))
            requestUrl += $"&license={license}";
        if (!string.IsNullOrWhiteSpace(categories))
            requestUrl += $"&categories={categories}";
        requestUrl += $"&page_size={pageSize}&page={page}";

        HttpResponseMessage response = await client.GetAsync(requestUrl);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to search audio: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to search audio");
        }
        var searchResponse =
            JsonSerializer.Deserialize<AudioSearchResponse>(responseContent, GetJsonSerializerOptions())!;
        return searchResponse;
    }
    public async Task<AudioResult?> GetAudioDetailAsync(string audioId)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/audio/{audioId}/");
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get audio detail: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get audio detail");
        }
        var detail = JsonSerializer.Deserialize<AudioResult>(responseContent, GetJsonSerializerOptions());
        return detail;
    }
    public async Task<AudioSearchResponse> GetRelatedAudioAsync(string audioId)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/audio/{audioId}/related/");
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get related audio: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get related audio");
        }
        var relatedResponse = JsonSerializer.Deserialize<AudioSearchResponse>(responseContent, GetJsonSerializerOptions())!;
        return relatedResponse;
    }
    public async Task<ImageSearchResponse> SearchImagesAsync(
        string query,
        string license = "",
        string categories = "",
        int pageSize = 20,
        int page = 1)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var requestUrl = $"/v1/images/?q={query}";
        if (!string.IsNullOrWhiteSpace(license))
            requestUrl += $"&license={license}";
        if (!string.IsNullOrWhiteSpace(categories))
            requestUrl += $"&categories={categories}";
        requestUrl += $"&page_size={pageSize}&page={page}";

        HttpResponseMessage response = await client.GetAsync(requestUrl);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to search images: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to search images");
        }

        var imageSearchResponse = JsonSerializer.Deserialize<ImageSearchResponse>(responseContent, GetJsonSerializerOptions())!;
        return imageSearchResponse;
    }
    public async Task<ImageResult?> GetImageDetailAsync(string imageId)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/images/{imageId}/");
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get image detail: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get image detail");
        }
        var imageDetail = JsonSerializer.Deserialize<ImageResult>(responseContent, GetJsonSerializerOptions());
        return imageDetail;
    }
    public async Task<ImageSearchResponse> GetRelatedImagesAsync(string imageId)
    {
        string token = await GetAuthToken();
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/images/{imageId}/related/");
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get related images: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get related images");
        }
        var relatedResponse = JsonSerializer.Deserialize<ImageSearchResponse>(responseContent, GetJsonSerializerOptions())!;
        return relatedResponse;
    }
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }
}
