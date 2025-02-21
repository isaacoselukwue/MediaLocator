using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ML.Application.Authentication.Commands.Login;
public class LoginDto
{
    public AccessTokenResponse? AccessToken { get; set; }
}
