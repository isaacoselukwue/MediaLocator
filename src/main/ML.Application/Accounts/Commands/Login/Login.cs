using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Commands.Login;
public record LoginCommand : IRequest<Result<LoginDto>>
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
}

internal class LoginCommandHandler (IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Result<LoginDto> result = await identityService.SignInUser(request.EmailAddress!, request.Password!);
        if(!result.Succeeded && string.Equals(result.Message, ""))
        {
            //maybe publish an event here for blocked users
        }
        return result;
    }
}
