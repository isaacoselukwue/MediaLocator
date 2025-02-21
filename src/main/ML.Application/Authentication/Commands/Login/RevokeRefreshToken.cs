using MediatR;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Authentication.Commands.Login;

public class RevokeRefreshTokenCommand : IRequest<Result>
{
    public string? EncryptedToken { get; set; }
}
internal class RevokeRefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await identityService.RevokeRefreshUserTokenAsync(request.EncryptedToken!);
    }
}