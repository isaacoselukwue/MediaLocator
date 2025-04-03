using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Media.Queries;

public record UsersSearchHistoryQuery : IRequest<Result<UsersSearchHistoryDto>>
{
    public string? Title { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsAscendingSorted { get; set; }
    public int PageNumber { get; set; } = 1;
}

public class UsersSearchHistoryValidator : AbstractValidator<UsersSearchHistoryQuery>
{
    public UsersSearchHistoryValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty().When(x => x.EndDate != null).WithMessage("Start date must be provided when end date is provided.");
        RuleFor(x => x.EndDate).NotEmpty().When(x => x.StartDate != null).WithMessage("End date must be provided when start date is provided.");
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0.");
    }
}

public class UsersSearchHistoryQueryHandler(ISearchService _searchService) : IRequestHandler<UsersSearchHistoryQuery, Result<UsersSearchHistoryDto>>
{
    public async Task<Result<UsersSearchHistoryDto>> Handle(UsersSearchHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = await _searchService.GetUsersSearchHistory(request.Title, request.StartDate, request.EndDate, request.IsAscendingSorted, request.PageNumber, cancellationToken);
        return result;
    }
}

public class UsersSearchHistoryDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public List<UsersSearchHistoryResult> Results { get; set; } = [];
}

public class UsersSearchHistoryResult
{
    public Guid Id { get; set; }
    public string? SearchId { get; set; }
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
    public DateTimeOffset SearchDate { get; set; }
}