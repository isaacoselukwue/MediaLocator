using Microsoft.AspNetCore.Identity;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Infrastructure.Identity;
internal class IdentityService(SignInManager<Users> signInManager, UserManager<Users> userManager) : IIdentityService
{
    public async Task<Result> SignInUser(string username, string password)
    {
        var result = await signInManager.PasswordSignInAsync(username, password, false, true);
        if (result.IsLockedOut)
        {
            return Result.Failure("Login in failed", ["Account might be locked out. Please retry in 24 hours"]);
        }
        else if (result.IsNotAllowed)
        {
            return Result.Failure("Login in failed", ["Please complete account sign up"]);
        }
        else if (!result.Succeeded)
        {
            return Result.Failure("Login in failed", ["Invalid username or password"]);
        }
        return Result.Success("Login in successful");
    }
}