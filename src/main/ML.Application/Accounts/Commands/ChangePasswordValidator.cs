namespace ML.Application.Accounts.Commands;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x=>x.NewPassword).NotEmpty();
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword);
    }
}
