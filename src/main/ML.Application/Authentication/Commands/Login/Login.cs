using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using ML.Domain.Enums;

namespace ML.Application.Authentication.Commands.Login;
public record LoginCommand : IRequest<Result<LoginDto>>
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
}

public class LoginCommandHandler (IIdentityService identityService, IPublisher publisher)
    : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Result<LoginDto> result = await identityService.SignInUserAsync(request.EmailAddress!, request.Password!);
        if(!result.Succeeded && result.Errors.Any(x=> x == ResultMessage.LoginFailedAccountLocked))
        {
            await publisher.Publish(new NotificationEvent(request.EmailAddress!, "Account Temporarily Disabled!", NotificationTypeEnum.SignInBlockedAccount, []), cancellationToken);
        }
        if (!result.Succeeded)
            return result;
        Dictionary<string, string> emailData = new()
        {
            {"{{date}}", DateTime.UtcNow.ToString("dd-MMM-yyyy") },
            {"{{time}}", DateTime.UtcNow.ToString("hh:mm tt") }
        };
        await publisher.Publish(new NotificationEvent(request.EmailAddress!, "Sign In Successful!", NotificationTypeEnum.SignInSuccess, emailData), cancellationToken);
        return result;
    }
}
