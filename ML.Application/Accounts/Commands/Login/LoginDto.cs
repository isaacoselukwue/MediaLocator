using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ML.Application.Accounts.Commands.Login;
public class LoginDto
{
    public AccessTokenResponse? AccessToken { get; set; }
}
