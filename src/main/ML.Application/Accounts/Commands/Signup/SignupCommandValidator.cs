global using FluentValidation;

namespace ML.Application.Accounts.Commands.Signup;
public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(x=>x.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x=>x.PhoneNumber).NotEmpty().MinimumLength(7).MinimumLength(14)
            .Matches(@"^[0]\d+$").WithMessage("Phone number must start with '0' and contain only digits"); ;
    }
}
