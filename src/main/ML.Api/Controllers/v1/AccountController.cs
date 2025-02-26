using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Accounts.Commands;
using ML.Application.Accounts.Queries;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
[ApiController]
[Authorize]
public class AccountController(ISender sender) : BaseController
{
    //change password
    [HttpPost("change-password")]
    public async ValueTask<ActionResult<Result>> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    //deactivate account
    [HttpDelete("deactivate-account")]
    public async ValueTask<ActionResult<Result>> DeactivateAccount()
    {
        var result = await sender.Send(new DeactivateAccountCommand());
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    //activate account - admin only
    [Authorize(Policy = "AdminPolicy")]
    [HttpPost("admin/activate-account")]
    public async ValueTask<ActionResult<Result>> ActivateAccount([FromBody] ActivateAccountCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
    //view users - admin only
    [Authorize(Policy = "AdminPolicy")]
    [HttpGet("admin/users")]
    public async ValueTask<ActionResult<Result>> ViewUsers()
    {
        var result = await sender.Send(new UserAccountQuery());
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
    //change user role - admin only
    [Authorize(Policy = "AdminPolicy")]
    [HttpPost("admin/change-role")]
    public async ValueTask<ActionResult<Result>> ChangeRole([FromBody] ChangeUserRoleCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    //delete account, pass users email for admin only add field (isPermamant and set default to false)
    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("admin/delete-account")]
    public async ValueTask<ActionResult<Result>> DeleteAccount([FromBody] DeleteAccountCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
