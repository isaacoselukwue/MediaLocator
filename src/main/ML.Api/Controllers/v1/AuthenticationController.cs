using MediatR;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
[ApiController]
public class AuthenticationController(ISender sender) : BaseController
{
    [HttpPost("login")]
    public  async ValueTask<ActionResult<Result<LoginDto>>> Login([FromBody] LoginCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
