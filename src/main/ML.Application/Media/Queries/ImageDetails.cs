using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Media.Queries;

public record ImageDetailsQuery : IRequest<Result<ImageSearchResult>>
{
    public string? Id { get; set; }
}

public class ImageDetailsValidator : AbstractValidator<ImageDetailsQuery>
{
    public ImageDetailsValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}

public class ImageDetailsHandler(ISearchService searchService) : IRequestHandler<ImageDetailsQuery, Result<ImageSearchResult>>
{
    public async Task<Result<ImageSearchResult>> Handle(ImageDetailsQuery request, CancellationToken cancellationToken)
    {
        var result = await searchService.GetImageDetails(request.Id!, cancellationToken);
        return result;
    }
}
