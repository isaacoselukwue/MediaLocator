using Microsoft.AspNetCore.Identity;
using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using System.Security.Claims;

namespace ML.Infrastructure.Identity;
internal class IdentityService(SignInManager<Users> signInManager, UserManager<Users> userManager, IJwtService jwtService) : IIdentityService
{
    public async Task<Result<LoginDto>> SignInUser(string username, string password)
    {
        var result = await signInManager.PasswordSignInAsync(username, password, false, true);
        if (result.IsLockedOut)
        {
            return Result<LoginDto>.Failure(ResultMessage.LoginFailedGeneric, [ResultMessage.LoginFailedAccountLocked]);
        }
        else if (result.IsNotAllowed)
        {
            return Result<LoginDto>.Failure(ResultMessage.LoginFailedGeneric, ["Please complete account sign up"]);
        }
        else if (!result.Succeeded)
        {
            return Result<LoginDto>.Failure(ResultMessage.LoginFailedGeneric, ["Invalid username or password"]);
        }

        Users? user = await userManager.FindByEmailAsync(username);
        if (user is null)
            return Result<LoginDto>.Failure(ResultMessage.LoginFailedGeneric, ["Invalid username or password"]);

        List<Claim> claims = (List<Claim>)await userManager.GetClaimsAsync(user);
        List<string> roles = (List<string>)await userManager.GetRolesAsync(user);

        return jwtService.GenerateToken(user, claims, roles);
    }
}