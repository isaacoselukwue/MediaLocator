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

public class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Result<LoginDto>>
{
    public async Task<Result<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Result<LoginDto> result = await identityService.RefreshUserTokenAsync(request.EncryptedToken!);
        return result;
    }
}
