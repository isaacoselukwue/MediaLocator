using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Accounts.Commands;

public class DeactivateAccountCommand : IRequest<Result>
{
}

internal class DeactivateAccountCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<DeactivateAccountCommand, Result>
{
    public async Task<Result> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.DeactivateAccountAsync();
        if(result.Item1.Succeeded)
        {
            await publisher.Publish(new NotificationEvent(result.usersEmail, "Sorry To See You Go!", NotificationTypeEnum.SignUpSuccess, []), cancellationToken);
        }
        return result.Item1;
    }
}
