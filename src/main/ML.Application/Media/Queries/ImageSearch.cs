using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Media.Queries;

public class ImageSearchQuery : IRequest<Result<ImageSearchDto>>
{
    public string? SearchQuery { get; set; }
    public OpenLicenseEnum? License { get; set; }
    public OpenLicenseTypeEnum? LicenseType { get; set; }
    public OpenImageCategoryEnum? Category { get; set; }
    public int PageNumber { get; set; }
}

public class ImageSearchValidator : AbstractValidator<ImageSearchQuery>
{
    public ImageSearchValidator()
    {
        RuleFor(x => x.SearchQuery).NotEmpty().WithMessage("Search query is required.");
        RuleFor(x => x.License).IsInEnum().WithMessage("License is not valid.");
        RuleFor(x => x.LicenseType).IsInEnum().WithMessage("License type is not valid.");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Category is not valid.");
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0.");
    }
}

internal class ImageSearchQueryHandler(ISearchService searchService) : IRequestHandler<ImageSearchQuery, Result<ImageSearchDto>>
{
    public async Task<Result<ImageSearchDto>> Handle(ImageSearchQuery request, CancellationToken cancellationToken)
    {
        var result = await searchService.SearchImage(request.SearchQuery!, request.License, request.LicenseType, request.Category, request.PageNumber, cancellationToken);
        return result;
    }
}

public class ImageSearchDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public List<ImageSearchResult> Results { get; set; } = new();
}

public class ImageSearchResult
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
    public string? FileType { get; set; }
    public string? ThumbNail { get; set; }
}