using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using ML.Domain.Enums;

namespace ML.Application.Accounts.Commands;

public record InitiatePasswordResetCommand : IRequest<Result>
{
    public string? EmailAddress { get; set; }
}

public class InitiatePasswordResetValidator : AbstractValidator<InitiatePasswordResetCommand>
{
    public InitiatePasswordResetValidator()
    {
        RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress();
    }
}

internal class InitiatePasswordResetCommandHandler(IIdentityService identityService, IPublisher publisher) : IRequestHandler<InitiatePasswordResetCommand, Result>
{
    public async Task<Result> Handle(InitiatePasswordResetCommand request, CancellationToken cancellationToken)
    {
        (Result result, string token) = await identityService.InitiateForgotPasswordAsync(request.EmailAddress!);
        if (!result.Succeeded || string.IsNullOrWhiteSpace(token))
            return result;

        Dictionary<string, string> emailData = new()
            {
                {"{{token}}", token },
                {"{{userid}}", result.Message }
            };
        await publisher.Publish(new NotificationEvent(request.EmailAddress!, "Password Reset Request", NotificationTypeEnum.PasswordResetInitiation, emailData), cancellationToken);
        return Result.Success(ResultMessage.ForgotPasswordSuccess);
    }
}
