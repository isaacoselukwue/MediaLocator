using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Accounts.Commands;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
[ApiController]
[Authorize]
public class AccountController(ISender sender) : BaseController
{
    //change password
    public async ValueTask<ActionResult<Result>> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    //deactivate account

    //delete account, pass users email for admin only
}
