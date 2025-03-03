using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Media.Commands;

public record DeleteSearchHistoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

public class DeleteSearchHistoryValidator : AbstractValidator<DeleteSearchHistoryCommand>
{
    public DeleteSearchHistoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal class DeleteSearchHistoryCommandHandler(ISearchService searchService) : IRequestHandler<DeleteSearchHistoryCommand, Result>
{
    public async Task<Result> Handle(DeleteSearchHistoryCommand request, CancellationToken cancellationToken)
    {
        var result = await searchService.DeleteSearchHistory(request.Id, cancellationToken);
        return result;
    }
}