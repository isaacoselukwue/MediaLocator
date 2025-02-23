using Microsoft.AspNetCore.Identity;
using ML.Application.Authentication.Commands.Login;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;
using ML.Domain.Constants;
using System.Security.Claims;

namespace ML.Infrastructure.Identity;
internal class IdentityService(SignInManager<Users> signInManager, UserManager<Users> userManager, IJwtService jwtService) : IIdentityService
{
    public async Task<Result> ChangePasswordAsync(string newPassword)
    {
        Users? user = await userManager.FindByIdAsync(jwtService.GetUserId().ToString());
        if (user is null)
        {
            return Result.Failure(ResultMessage.ChangePasswordFailed, ["Invalid user"]);
        }
        if (user.UsersStatus != Domain.Enums.StatusEnum.Active)
            return Result.Failure(ResultMessage.ChangePasswordFailed, ["Account is not active"]);

        IdentityResult result = await userManager.ChangePasswordAsync(user, user.PasswordHash!, newPassword);
        if (!result.Succeeded)
        {
            return result.ToApplicationResult(ResultMessage.ChangePasswordFailed);
        }
        await signInManager.RefreshSignInAsync(user);
        return Result.Success(ResultMessage.ChangePasswordSuccess);
    }
    public async Task<(Result, string usersEmail)> DeactivateAccountAsync()
    {
        Users? user = await userManager.FindByIdAsync(jwtService.GetUserId().ToString());
        if (user is null)
        {
            return (Result.Failure(ResultMessage.DeactivateAccountFailed, ["Invalid user"]), string.Empty);
        }
        if (user.UsersStatus != Domain.Enums.StatusEnum.Active)
            return (Result.Failure(ResultMessage.DeactivateAccountFailed, ["Account is not active"]), string.Empty);
        user.UsersStatus = Domain.Enums.StatusEnum.InActive;
        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (result.ToApplicationResult(ResultMessage.DeactivateAccountFailed), string.Empty);
        }
        return (Result.Success(ResultMessage.DeactivateAccountSuccess), user.Email!);
    }
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

        if(user.UsersStatus != Domain.Enums.StatusEnum.Active)
            return Result<LoginDto>.Failure(ResultMessage.LoginFailedGeneric, ["Account is not active"]);

        List<Claim> claims = (List<Claim>)await userManager.GetClaimsAsync(user);
        List<string> roles = (List<string>)await userManager.GetRolesAsync(user);

        var token = jwtService.GenerateToken(user, claims, roles);
        if(token.Succeeded)
            await userManager.SetAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken", token.Data!.AccessToken!.RefreshToken);

        return token;
    }
    public async Task<Result<LoginDto>> RefreshUserTokenAsync(string encryptedToken)
    {
        (string token, string userId) = jwtService.UnprotectToken(encryptedToken);
        Users? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result<LoginDto>.Failure(ResultMessage.TokenRefreshFailed, ["Invalid user"]);
        }
        if(user.UsersStatus != Domain.Enums.StatusEnum.Active)
        {
            return Result<LoginDto>.Failure(ResultMessage.TokenRefreshFailed, ["Account is not active"]);
        }

        string? validToken = await userManager.GetAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken");
        if (validToken != encryptedToken)
        {
            return Result<LoginDto>.Failure(ResultMessage.TokenRefreshFailed, ["Invalid token"]);
        }

        List<Claim> claims = (List<Claim>)await userManager.GetClaimsAsync(user);
        List<string> roles = (List<string>)await userManager.GetRolesAsync(user);

        var tokenResult = jwtService.GenerateToken(user, claims, roles);
        if (tokenResult.Succeeded)
        {
            await userManager.RemoveAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken");
            await userManager.SetAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken", tokenResult.Data!.AccessToken!.RefreshToken);
        }

        return tokenResult;
    }
    public async Task<Result> RevokeRefreshUserTokenAsync(string encryptedToken)
    {
        (string token, string userId) = jwtService.UnprotectToken(encryptedToken);
        Users? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(ResultMessage.TokenRefreshFailed, ["Invalid user"]);
        }
        if (user.UsersStatus != Domain.Enums.StatusEnum.Active)
        {
            return Result.Failure(ResultMessage.TokenRefreshFailed, ["Account is not active"]);
        }

        string? validToken = await userManager.GetAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken");
        if (validToken != encryptedToken)
        {
            return Result.Failure(ResultMessage.TokenRefreshFailed, ["Invalid token"]);
        }

        await userManager.RemoveAuthenticationTokenAsync(user, "MediaLocator", "RefreshToken");

        return Result.Success("Refresh token successfully revoked");
    }
    public async Task<(Result, string token)> SignUpUserAsync(string email, string password, string firstName, string lastName, string phoneNumber)
    {
        Users user = new()
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            EmailConfirmed = false,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = email,
            LastModifiedBy = email
        };

        IdentityResult result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (result.ToApplicationResult(ResultMessage.SignUpFailed), string.Empty);
        }
        IdentityResult roleResult = await userManager.AddToRoleAsync(user, Roles.User);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return (roleResult.ToApplicationResult(ResultMessage.SignUpFailed), string.Empty);
        }

        IdentityResult claimResult = await userManager.AddClaimAsync(user, new Claim("Permission", "CanView"));
        if (!claimResult.Succeeded)
        {
            await userManager.RemoveFromRoleAsync(user, Roles.User);
            await userManager.DeleteAsync(user);
            return (claimResult.ToApplicationResult(ResultMessage.SignUpFailed), string.Empty);
        }

        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        return (result.ToApplicationResult(user.Id.ToString()), token);
    }
    public async Task<(Result, string usersEmail)> ValidateSignupAsync(string userId, string activationToken)
    {
        Users? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (Result.Failure(ResultMessage.SignUpFailed, ["Invalid user"]), string.Empty);
        }
        if (user.UsersStatus != Domain.Enums.StatusEnum.Pending)
        {
            return (Result.Failure(ResultMessage.SignUpFailed, ["Invalid user"]), string.Empty);
        }
        if (user.EmailConfirmed)
        {
            return (Result.Failure(ResultMessage.SignUpFailed, ["User already activated"]), string.Empty);
        }
        IdentityResult result = await userManager.ConfirmEmailAsync(user, activationToken);
        if (!result.Succeeded)
        {
            return (result.ToApplicationResult(ResultMessage.SignUpFailed), string.Empty);
        }
        return (Result.Success(ResultMessage.SignUpSuccess), user.Email!);
    }
}