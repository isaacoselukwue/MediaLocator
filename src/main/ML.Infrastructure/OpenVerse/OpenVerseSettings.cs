namespace ML.Infrastructure.OpenVerse;
public class OpenVerseSettings
{
    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }
}
