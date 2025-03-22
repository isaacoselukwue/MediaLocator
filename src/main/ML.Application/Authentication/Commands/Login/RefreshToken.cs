using MediatR;
using Microsoft.Extensions.Logging;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using System.Text.Json;

namespace ML.Application.Authentication.Commands.Login;
public record RefreshTokenCommand : IRequest<Result<LoginDto>>
{
    public string? EncryptedToken { get; set; }
}

internal class RefreshTokenCommandHandler(IIdentityService identityService, ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<LoginDto>>
{
    public async Task<Result<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Result<LoginDto> result = await identityService.RefreshUserTokenAsync(request.EncryptedToken!);
        //remember to remove ooo
        logger.LogInformation("Refresh token for checks: {request} and response: {response}", JsonSerializer.Serialize(request), JsonSerializer.Serialize(result));
        return result;
    }
}
