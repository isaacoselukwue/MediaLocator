namespace ML.Application.Accounts.Commands.Login;
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.EncryptedToken).NotEmpty();
    }
}