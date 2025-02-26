using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Commands;

public class DeleteAccountCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public bool IsPermanant { get; set; }
}

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
    }
}

internal class DeleteAccountCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.DeleteUserAsync(request.UserId.ToString(), request.IsPermanant);
        return result;
    }
}
