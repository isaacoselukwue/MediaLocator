global using System.Text.Json.Serialization;

internal class AudioSearchResponse
{
    [JsonPropertyName("result_count")]
    public int ResultCount { get; set; }
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("results")]
    public List<AudioResult>? Results { get; set; }
    [JsonPropertyName("warnings")]
    public List<AudioSearchWarning>? Warnings { get; set; }
}

internal class AudioSearchWarning
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

internal class AudioResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("indexed_on")]
    public DateTime? IndexedOn { get; set; }
    [JsonPropertyName("foreign_landing_url")]
    public string? ForeignLandingUrl { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("creator")]
    public string? Creator { get; set; }
    [JsonPropertyName("creator_url")]
    public string? CreatorUrl { get; set; }
    [JsonPropertyName("license")]
    public string? License { get; set; }
    [JsonPropertyName("license_version")]
    public string? LicenseVersion { get; set; }
    [JsonPropertyName("license_url")]
    public string? LicenseUrl { get; set; }
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }
    [JsonPropertyName("filesize")]
    public int? FileSize { get; set; }
    [JsonPropertyName("filetype")]
    public string? FileType { get; set; }
    [JsonPropertyName("tags")]
    public List<AudioTag>? Tags { get; set; }
    [JsonPropertyName("alt_files")]
    public List<AudioAltFile>? AltFiles { get; set; }
    [JsonPropertyName("attribution")]
    public string? Attribution { get; set; }
    [JsonPropertyName("fields_matched")]
    public List<string>? FieldsMatched { get; set; }
    [JsonPropertyName("mature")]
    public bool? Mature { get; set; }
    [JsonPropertyName("audio_set")]
    public AudioSet? AudioSet { get; set; }
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }
    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
    [JsonPropertyName("detail_url")]
    public string? DetailUrl { get; set; }
    [JsonPropertyName("related_url")]
    public string? RelatedUrl { get; set; }
    [JsonPropertyName("waveform")]
    public string? Waveform { get; set; }
    [JsonPropertyName("unstable__sensitivity")]
    public List<string>? UnstableSensitivity { get; set; }
}

internal class AudioTag
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("accuracy")]
    public int? Accuracy { get; set; }
    [JsonPropertyName("unstable__provider")]
    public string? UnstableProvider { get; set; }
}

internal class AudioAltFile
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }
    [JsonPropertyName("filesize")]
    public int? FileSize { get; set; }
    [JsonPropertyName("filetype")]
    public string? FileType { get; set; }
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }
}

internal class AudioSet
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("foreign_landing_url")]
    public string? ForeignLandingUrl { get; set; }
    [JsonPropertyName("creator")]
    public string? Creator { get; set; }
    [JsonPropertyName("creator_url")]
    public string? CreatorUrl { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("filesize")]
    public int? FileSize { get; set; }
    [JsonPropertyName("filetype")]
    public string? FileType { get; set; }
}
