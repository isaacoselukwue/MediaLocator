namespace ML.Web.Models;

public class AudioSearchDto
{
    public List<AudioSearchResult> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class ImageSearchDto
{
    public List<ImageSearchResult> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class AudioSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ThumbNail { get; set; } = string.Empty;
    public string? Attribuition { get; set; }
    public string? Provider { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public List<string> Genres { get; set; } = [];
    public string? CreatorUrl { get; set; }
    public string? ForeignLandingUrl { get; set; }
    public string? LicenseVersion { get; set; }
    public string? LicenseUrl { get; set; }
    public int? FileSize { get; set; }
}

public class ImageSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ThumbNail { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? Attribuition { get; set; }
    public string? Provider { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public string? CreatorUrl { get; set; }
    public string? ForeignLandingUrl { get; set; }
    public string? LicenseVersion { get; set; }
    public string? LicenseUrl { get; set; }
}

public class PaginatedSearchHistoryDto
{
    public List<UsersSearchHistoryResult> Results { get; set; } = new();
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class UsersSearchHistoryResult
{
    public string Id { get; set; } = string.Empty;
    public string SearchId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime SearchDateTime { get; set; }
    public string ThumbNail { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? LicenseUrl { get; set; }
    public string? CreatorUrl { get; set; }
    public string? LicenseVersion { get; set; }
    public string? Category { get; set; }
    public string? Provider { get; set; }
    public string? Source { get; set; }
    public string? ForeignLandingUrl { get; set; }
}

public class AddSearchHistoryRequest
{
    public string SearchId { get; set; } = string.Empty;
    public SearchTypeEnum SearchType { get; set; }
}
public enum OpenLicenseEnum
{
    By,
    ByNc,
    ByNcNd,
    ByNcSa,
    ByNd,
    BySa,
    Cc0,
    Pdm,
    NcSamplingPlus,
    SamplingPlus
}

public enum OpenLicenseTypeEnum
{
    All,
    Commercial,
    Modification,
    AllCc
}

public enum OpenAudioCategoryEnum
{
    Audiobook,
    Music,
    News,
    Podcast,
    Pronunciation,
    SoundEffect
}

public enum OpenImageCategoryEnum
{
    DigitalisedArtwork,
    Illustration,
    Photograph
}
public enum SearchTypeEnum
{
    Audio = 1,
    Image
}