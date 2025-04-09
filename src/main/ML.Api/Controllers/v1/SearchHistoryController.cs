using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Common.Models;
using ML.Domain.Entities;
using ML.Domain.Enums;
using ML.Domain.Events;

namespace ML.Api.Controllers.v1;
[ApiController]
[AllowAnonymous]
public class SearchHistoryController(IPublisher publisher, UserManager<Users> userManager) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<Result>> TestPublisher()
    {
        await publisher.Publish(new NotificationEvent("princeizak@live.com", "Account Activated", NotificationTypeEnum.AccountActivationAdmin, []), CancellationToken.None);
        return Ok();
    }
    //add to search history - user
    [HttpPost("test")]
    public async ValueTask<ActionResult<string>> TestAuth()
    {
        Users user = await userManager.FindByEmailAsync("princeizak@live.com") ?? new();
        //await userManager.
        Console.WriteLine("Here");
        await Task.CompletedTask;
        return Ok();
    }
    //fetch search history - user

    //delete search history - user

    //view search history - admin
}
