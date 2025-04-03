using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Enums;

namespace ML.Application.Authentication.Commands.Signup;
public record SignupCommand : IRequest<Result>
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class SignupCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<SignupCommand, Result>
{
    public async Task<Result> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.SignUpUserAsync(request.EmailAddress!, request.Password!, request.FirstName!, request.LastName!, request.PhoneNumber!);

        if(!result.Item1.Succeeded)
        {
            return result.Item1;
        }
        //send email with activation link via event
        Dictionary<string, string> emailData = new()
        {
            {"{{token}}", result.token },
            {"{{userid}}", result.Item1.Message }
        };
        await publisher.Publish(new NotificationEvent(request.EmailAddress!, "Account Activation!", NotificationTypeEnum.SignUpAccountActivation, emailData), cancellationToken);
        return Result.Success("Signup successful. Please check your mail for activation link.");
    }
}