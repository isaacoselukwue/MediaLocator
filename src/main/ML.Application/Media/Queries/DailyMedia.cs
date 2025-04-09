using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Media.Queries;

public record DailyMediaQuery : IRequest<Result<DailyMediaDto>>
{
}

public class DailyMediaQueryHandler(ISearchService searchService) : IRequestHandler<DailyMediaQuery, Result<DailyMediaDto>>
{
    public async Task<Result<DailyMediaDto>> Handle(DailyMediaQuery request, CancellationToken cancellationToken)
    {
        var result = await searchService.GetDailyMediaAsync(cancellationToken);
        return result;
    }
}

public class DailyMediaDto
{
    public string? WordOfTheDay { get; set; }
    public List<AudioSearchResult> AudioSearchResults { get; set; } = [];
    public List<ImageSearchResult> ImageSearchResults { get; set; } = [];

}