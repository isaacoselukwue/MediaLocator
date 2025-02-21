namespace ML.Application.Accounts.Commands;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x=>x.NewPassword).NotEmpty();
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword);
    }
}
