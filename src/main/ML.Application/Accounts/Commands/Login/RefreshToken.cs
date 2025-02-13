using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Commands.Login;
public record RefreshTokenCommand : IRequest<Result<LoginDto>>
{
    public string? EncryptedToken { get; set; }
}

internal class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Result<LoginDto>>
{
    public async Task<Result<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Result<LoginDto> result = await identityService.RefreshUserTokenAsync(request.EncryptedToken!);
        return result;
    }
}
