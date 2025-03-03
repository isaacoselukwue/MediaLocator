using ML.Domain.Common;
using ML.Domain.Enums;

namespace ML.Domain.Entities;
public class SearchHistories : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Users? User { get; set; }
    public string? SearchId { get; set; }
    public SearchTypeEnum SearchType { get; set; }
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
    public StatusEnum Status { get; set; }
}
