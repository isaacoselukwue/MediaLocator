using MediatR;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Authentication.Commands.Login;
using ML.Application.Authentication.Commands.Signup;
using ML.Application.Common.Models;

namespace ML.Api.Controllers.v1;
[ApiController]
public class AuthenticationController(ISender sender) : BaseController
{
    /// <summary>
    /// Authenticates a user with the given credentials.
    /// </summary>
    /// <param name="command">The login command including email address and password.</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{LoginDto}"/> with an access token if login is successful;
    /// otherwise, a failure message.
    /// </returns>
    /// <response code="200">Login successful and access token is returned.</response>
    /// <response code="400">Login failed due to invalid credentials or account restrictions.</response>
    [HttpPost("login")]
    public async ValueTask<ActionResult<Result<LoginDto>>> Login([FromBody] LoginCommand command)
    {
        var result = await sender.Send(command);
        AddRefreshToken(result);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login/refresh")]
    public async ValueTask<ActionResult<Result<LoginDto>>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await sender.Send(command);
        AddRefreshToken(result);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("logout")]
    public async ValueTask<ActionResult<Result>> Logout([FromBody] RevokeRefreshTokenCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// Registers a new user and sends an activation email.
    /// </summary>
    /// <param name="command">
    /// The signup command including email address, password, confirm password, first name, last name, and phone number.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result"/> indicating whether the signup was successful.
    /// In case of success, an activation token is generated and an event is published to send an activation email.
    /// </returns>
    /// <response code="200">Signup successful; an activation email is sent to the user.</response>
    /// <response code="400">Signup failed due to validation errors or account duplication.</response>
    [HttpPost("signup")]
    public async ValueTask<ActionResult<Result>> SignUp([FromBody] SignupCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
    [HttpPost("signup/verify")]
    public async ValueTask<ActionResult<Result>> VerifySignup([FromBody] SignupVerificationCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    private void AddRefreshToken(Result<LoginDto> result)
    {
        if (result.Succeeded && result.Data is not null && result.Data.AccessToken is not null)
        {
            Response.Cookies.Append("refreshToken", result.Data.AccessToken.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMonths(1)
            });
        }
    }
}
