using MediatR;
using Microsoft.AspNetCore.Http.Timeouts;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Commands;

public record ActivateAccountCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

public class ActivateAccountValidator : AbstractValidator<ActivateAccountCommand>
{
    public ActivateAccountValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
    }
}

internal class ActivateAccountCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<ActivateAccountCommand, Result>
{
    public async Task<Result> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ActivateAccountAsync(request.UserId);
        return result.Item1;
    }
}
