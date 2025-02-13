namespace ML.Application.Accounts.Commands.Signup;
internal class SignupVerificationValidator : AbstractValidator<SignupVerificationCommand>
{
    public SignupVerificationValidator()
    {
        RuleFor(x=>x.UserId).NotEmpty().Must(BeAValidGuid).WithMessage("Invalid user");
        RuleFor(x => x.ActivationToken).NotEmpty();
    }
    private bool BeAValidGuid(string? guid)
    {
        if(string.IsNullOrWhiteSpace(guid)) return false;
        return Guid.TryParse(guid, out _);
    }
}
