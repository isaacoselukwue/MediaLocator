using ML.Application.Accounts.Commands.Login;
using ML.Application.Common.Models;
using ML.Domain.Entities;
using System.Security.Claims;

namespace ML.Application.Common.Interfaces;
public interface IJwtService
{
    Result<LoginDto> GenerateToken(Users user, List<Claim> userClaims, List<string> roles);
    (string token, string userId) UnprotectToken(string protectedToken);
}
