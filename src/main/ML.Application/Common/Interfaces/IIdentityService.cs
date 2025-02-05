using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;

namespace ML.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result<LoginDto>> SignInUser(string username, string password);
}
