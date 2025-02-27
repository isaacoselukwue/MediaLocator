using System.Text.Json.Serialization;

namespace ML.Infrastructure.OpenVerse.DTOs;
internal class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
public class RateLimitResponse
{
    [JsonPropertyName("requests_this_minute")]
    public int? RequestsThisMinute { get; set; }
    [JsonPropertyName("requests_today")]
    public int? RequestsToday { get; set; }
    [JsonPropertyName("rate_limit_model")]
    public string? RateLimitModel { get; set; }
    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }
}