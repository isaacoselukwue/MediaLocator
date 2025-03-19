namespace ML.Web.Models;

public class LoginRequest
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
}

public class LoginResponse
{
    public AccessTokenResponse? AccessToken { get; set; }
}

public class AccessTokenResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
    public string? TokenType { get; set; }
}

public class RefreshTokenRequest
{
    public string? EncryptedToken { get; set; }
}

public class RevokeTokenRequest
{
    public string? EncryptedToken { get; set; }
}

public class ApiResult
{
    public bool Succeeded { get; set; }
    public string[]? Errors { get; set; }
    public string? Message { get; set; }
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }
}

public class ErrorResponse
{
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
}
public class SignupRequest
{
    public string? EmailAddress { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class SignupVerificationRequest
{
    public string? UserId { get; set; }
    public string? ActivationToken { get; set; }
}