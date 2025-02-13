using ML.Domain.Common;

namespace ML.Domain.Entities;
public class SearchHistories : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string? OpenVerseId { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Creator { get; set; }
    public string? License { get; set; }
    public string? Provider { get; set; }
    public string? Attribuition { get; set; }
    public string? RelatedUrl { get; set; }
}
