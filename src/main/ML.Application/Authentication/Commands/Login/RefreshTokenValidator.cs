namespace ML.Application.Authentication.Commands.Login;
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.EncryptedToken).NotEmpty();
    }
}