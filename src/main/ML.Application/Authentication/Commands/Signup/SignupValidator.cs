global using FluentValidation;

namespace ML.Application.Authentication.Commands.Signup;
public class SignupValidator : AbstractValidator<SignupCommand>
{
    public SignupValidator()
    {
        RuleFor(x=>x.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x=>x.PhoneNumber).NotEmpty().MinimumLength(7).MaximumLength(14)
            .Matches(@"^[0]\d+$").WithMessage("Phone number must start with '0' and contain only digits"); ;
    }
}
