namespace ML.Web.Models;

public class AdminSearchHistoryDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public List<AdminSearchHistoryResult> Results { get; set; } = [];
}

public class AdminSearchHistoryResult
{
    public Guid Id { get; set; }
    public string? SearchId { get; set; }
    public Guid UserId { get; set; }
    public string? UsersEmail { get; set; }
    public int Status { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Creator { get; set; }
    public string? License { get; set; }
    public string? Provider { get; set; }
    public string? Attribuition { get; set; }
    public string? RelatedUrl { get; set; }
    public DateTime? IndexedOn { get; set; }
    public string? ForeignLandingUrl { get; set; }
    public string? CreatorUrl { get; set; }
    public string? LicenseVersion { get; set; }
    public string? LicenseUrl { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public string? Genres { get; set; }
    public int? FileSize { get; set; }
    public string? FileType { get; set; }
    public string? ThumbNail { get; set; }
    public DateTimeOffset? SearchDate { get; set; }
}