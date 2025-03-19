using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Media.Queries;

public record AudioSearchQuery : IRequest<Result>
{
    public string? SearchQuery { get; set; }
    public OpenLicenseEnum? License { get; set; }
    public OpenLicenseTypeEnum? LicenseType { get; set; }
    public OpenAudioCategoryEnum? Category { get; set; }
    public int PageNumber { get; set; } = 1;
}

public class AudioSearchValidator : AbstractValidator<AudioSearchQuery>
{
    public AudioSearchValidator()
    {
        RuleFor(x => x.SearchQuery).NotEmpty().WithMessage("Search query is required.");
        RuleFor(x => x.License).IsInEnum().WithMessage("License is not valid.");
        RuleFor(x => x.LicenseType).IsInEnum().WithMessage("License type is not valid.");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Category is not valid.");
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0.");
    }
}

internal class AudioSearchQueryHandler(ISearchService searchService) : IRequestHandler<AudioSearchQuery, Result>
{
    public async Task<Result> Handle(AudioSearchQuery request, CancellationToken cancellationToken)
    {
        var result = await searchService.SearchAudio(request.SearchQuery!, request.License, request.LicenseType, request.Category, request.PageNumber, cancellationToken);
        return result;
    }
}

public class AudioSearchDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public List<AudioSearchResult> Results { get; set; } = [];
}

public class AudioSearchResult
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public DateTime? IndexedOn { get; set; }
    public string? ForeignLandingUrl { get; set; }
    public string? Url { get; set; }
    public string? Creator { get; set; }
    public string? CreatorUrl { get; set; }
    public string? License { get; set; }
    public string? LicenseVersion { get; set; }
    public string? LicenseUrl { get; set; }
    public string? Provider { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public List<string> Genres { get; set; } = [];
    public int? FileSize { get; set; }
    public string? FileType { get; set; }
    public string? RelatedUrl { get; set; }
    public string? Attribution { get; set; }
    public string? Thumbnail { get; set; }
}