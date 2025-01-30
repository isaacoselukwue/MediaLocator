using MediatR;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
[ApiController]
public class AuthenticationController(IMediator mediator) : BaseController
{
    [HttpPost("login")]
    public  async ValueTask<ActionResult<Result<LoginDto>>> Login([FromBody] LoginCommand command)
    {
        return Ok(await mediator.Send(command));
    }
}
