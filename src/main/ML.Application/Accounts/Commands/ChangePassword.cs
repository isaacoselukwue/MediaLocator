using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Accounts.Commands;

public record ChangePasswordCommand : IRequest<Result>
{
    public string? NewPassword { get; set; }
    public string? ConfirmNewPassword { get; set; }
}

internal class ChangePasswordCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ChangePasswordAsync(request.NewPassword!);
        if(result.Item1.Succeeded)
        {
            await publisher.Publish(new NotificationEvent(result.email, "Password Changed", NotificationTypeEnum.ChangePasswordSuccess, []), cancellationToken);
        }
        return result.Item1;
    }
}
