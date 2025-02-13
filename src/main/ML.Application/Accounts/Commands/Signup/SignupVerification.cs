using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Accounts.Commands.Signup;
public record SignupVerificationCommand : IRequest<Result>
{
    public string? UserId { get; set; }
    public string? ActivationToken { get; set; }
}

internal class SignupVerificationCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<SignupVerificationCommand, Result>
{
    public async Task<Result> Handle(SignupVerificationCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ValidateSignupAsync(request.UserId!, request.ActivationToken!);
        if(!result.Item1.Succeeded) return result.Item1;

        Dictionary<string, string> emailData = new()
        {
            {"email", result.usersEmail }
        };
        await publisher.Publish(new NotificationEvent(result.usersEmail!, "Account Activation Succeeded!", NotificationTypeEnum.SignUpActivationSuccess, emailData), cancellationToken);
        return result.Item1;
    }
}