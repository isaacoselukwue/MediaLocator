namespace ML.Application.Authentication.Commands.Login;

public class RevokeRefreshTokenValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenValidator()
    {
        RuleFor(x => x.EncryptedToken).NotEmpty();
    }
}
