using ML.Application.Authentication.Commands.Login;
using ML.Application.Common.Models;

namespace ML.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result> ChangePasswordAsync(string newPassword);
    Task<(Result, string usersEmail)> DeactivateAccountAsync();
    Task<Result<LoginDto>> RefreshUserTokenAsync(string encryptedToken);
    Task<Result> RevokeRefreshUserTokenAsync(string encryptedToken);
    Task<Result<LoginDto>> SignInUser(string username, string password);
    Task<(Result, string token)> SignUpUserAsync(string email, string password, string firstName, string lastName, string phoneNumber);
    Task<(Result, string usersEmail)> ValidateSignupAsync(string userId, string activationToken);
}
