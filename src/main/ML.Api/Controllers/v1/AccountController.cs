using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Accounts.Commands;
using ML.Application.Accounts.Queries;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
/// <summary>
/// Account controller for managing user accounts. Here users can change password, deactivate account. Admins can activate account, view users, change user role and delete account.
/// </summary>
/// <param name="sender"></param>
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
    public async ValueTask<ActionResult<Result>> ViewUsers([FromQuery] UserAccountQuery query)
    {
        var result = await sender.Send(query);
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

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("admin/deactivate-account")]
    public async ValueTask<ActionResult<Result>> DeactivateAccount([FromBody] DeactivateAccountAdminCommand command)
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

    [AllowAnonymous]
    [HttpPost("password-reset/initial")]
    public async ValueTask<ActionResult<Result>> PasswordResetInitial([FromBody] InitiatePasswordResetCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("password-reset")]
    public async ValueTask<ActionResult<Result>> PasswordReset([FromBody] PasswordResetCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
