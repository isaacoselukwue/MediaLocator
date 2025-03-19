using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ML.Infrastructure.OpenVerse.DTOs;
using System.Text.Json;
using RequestException = ML.Application.Common.Exceptions.HttpRequestException;

namespace ML.Infrastructure.OpenVerse;

public interface IOpenVerseService
{
    Task<AudioResult?> GetAudioDetailAsync(string audioId, CancellationToken cancellationToken);
    Task<ImageResult?> GetImageDetailAsync(string imageId, CancellationToken cancellationToken);
    Task<ImageSearchResponse> SearchImagesAsync(
        string query,
        CancellationToken cancellationToken,
        string license = "",
        string licenseType = "",
        string categories = "",
        int pageSize = 21,
        int page = 1);
    Task<AudioSearchResponse> SearchAudioAsync(
        string query,
        CancellationToken cancellationToken,
        string license = "",
        string licenseType = "",
        string categories = "",
        int pageSize = 21,
        int page = 1);
}


public class OpenVerseService(IHttpClientFactory httpClientFactory, ILogger<OpenVerseService> logger, HybridCache hybridCache, IOptions<OpenVerseSettings> openVerseSettings) 
    : IOpenVerseService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly OpenVerseSettings openVerseSettings = openVerseSettings.Value;

    private HttpClient InitialiseClient()
    {
        return _httpClientFactory.CreateClient("OpenVerseClient");
    }
    public async Task<string> GetAuthToken(CancellationToken cancellationToken)
    {
        string authToken = await hybridCache.GetOrCreateAsync(nameof(OpenVerseSettings), async token =>
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
                HttpResponseMessage response = await client.SendAsync(requestMessage, token);
                string responseContent = await response.Content.ReadAsStringAsync(token);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Failed to get auth token: {responseContent}, {statusCode}, {reasonPhrase}", responseContent, response.StatusCode, response.ReasonPhrase);
                    throw new RequestException("Failed to get auth token");
                }
                TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, GetJsonSerializerOptions())!;
                return tokenResponse.AccessToken!;
            }, 
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromSeconds(3600), LocalCacheExpiration = TimeSpan.FromSeconds(3600)
        }, cancellationToken: cancellationToken);
        return authToken;
    }
    public async Task<RateLimitResponse> GetRateLimitAsync(CancellationToken cancellationToken)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync("/v1/rate_limit", cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
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
        CancellationToken cancellationToken,
        string license = "",
        string licenseType = "",
        string categories = "",
        int pageSize = 21,
        int page = 1)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var requestUrl = "/v1/audio/?q=" + query;
        if (!string.IsNullOrWhiteSpace(license))
            requestUrl += $"&license={license}";
        if (!string.IsNullOrWhiteSpace(licenseType))
            requestUrl += $"&license_type={licenseType}";
        if (!string.IsNullOrWhiteSpace(categories))
            requestUrl += $"&categories={categories}";
        requestUrl += $"&page_size={pageSize}&page={page}";

        HttpResponseMessage response = await client.GetAsync(requestUrl, cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
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
    public async Task<AudioResult?> GetAudioDetailAsync(string audioId, CancellationToken cancellationToken)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/audio/{audioId}/", cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get audio detail: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get audio detail");
        }
        var detail = JsonSerializer.Deserialize<AudioResult>(responseContent, GetJsonSerializerOptions());
        return detail;
    }
    public async Task<AudioSearchResponse> GetRelatedAudioAsync(string audioId, CancellationToken cancellationToken)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/audio/{audioId}/related/", cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
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
        CancellationToken cancellationToken,
        string license = "",
        string licenseType = "",
        string categories = "",
        int pageSize = 21,
        int page = 1)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var requestUrl = $"/v1/images/?q={query}";
        if (!string.IsNullOrWhiteSpace(license))
            requestUrl += $"&license={license}";
        if (!string.IsNullOrWhiteSpace(licenseType))
            requestUrl += $"&license_type={licenseType}";
        if (!string.IsNullOrWhiteSpace(categories))
            requestUrl += $"&categories={categories}";
        requestUrl += $"&page_size={pageSize}&page={page}";

        HttpResponseMessage response = await client.GetAsync(requestUrl, cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to search images: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to search images");
        }

        var imageSearchResponse = JsonSerializer.Deserialize<ImageSearchResponse>(responseContent, GetJsonSerializerOptions())!;
        return imageSearchResponse;
    }
    public async Task<ImageResult?> GetImageDetailAsync(string imageId, CancellationToken cancellationToken)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/images/{imageId}/", cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Failed to get image detail: {responseContent}, {statusCode}, {reasonPhrase}",
                responseContent, response.StatusCode, response.ReasonPhrase);
            throw new RequestException("Failed to get image detail");
        }
        var imageDetail = JsonSerializer.Deserialize<ImageResult>(responseContent, GetJsonSerializerOptions());
        return imageDetail;
    }
    public async Task<ImageSearchResponse> GetRelatedImagesAsync(string imageId, CancellationToken cancellationToken)
    {
        string token = await GetAuthToken(cancellationToken);
        HttpClient client = InitialiseClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync($"/v1/images/{imageId}/related/", cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
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
