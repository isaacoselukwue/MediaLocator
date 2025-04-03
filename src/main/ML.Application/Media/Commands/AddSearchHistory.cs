using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Media.Commands;

public record AddSearchHistoryCommand : IRequest<Result>
{
    public string? SearchId { get; set; }
    public SearchTypeEnum SearchType { get; set; }
}

public class AddSearchHistoryValidator : AbstractValidator<AddSearchHistoryCommand>
{
    public AddSearchHistoryValidator()
    {
        RuleFor(x => x.SearchId).NotEmpty();
        RuleFor(x => x.SearchType).IsInEnum();
    }
}

public class AddSearchHistoryCommandValidator(ISearchService searchService) : IRequestHandler<AddSearchHistoryCommand, Result>
{
    public async Task<Result> Handle(AddSearchHistoryCommand request, CancellationToken cancellationToken)
    {
        var result = await searchService.AddSearchHistory(request.SearchId!, request.SearchType, cancellationToken);
        return result;
    }
}