using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Media.Queries;

public record AudioDetailsQuery : IRequest<Result<AudioSearchResult>>
{
    public string? Id { get; set; }
}
public class AudioDetailsValidator : AbstractValidator<AudioDetailsQuery>
{
    public AudioDetailsValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
public class AudioDetailsQueryHandler(ISearchService searchService) : IRequestHandler<AudioDetailsQuery, Result<AudioSearchResult>>
{
    public async Task<Result<AudioSearchResult>> Handle(AudioDetailsQuery request, CancellationToken cancellationToken)
    {
        var result = await searchService.GetAudioDetails(request.Id!, cancellationToken);
        return result;
    }
}