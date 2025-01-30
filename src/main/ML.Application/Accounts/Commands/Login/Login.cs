using MediatR;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Commands.Login;
public record LoginCommand : IRequest<Result<LoginDto>>
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
}
