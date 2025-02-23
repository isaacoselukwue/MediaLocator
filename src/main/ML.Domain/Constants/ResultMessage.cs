namespace ML.Domain.Constants;
public class ResultMessage
{
    public const string AccessTokenGenerated = "Access token generated successfully.";
    public const string ChangePasswordFailed = "You cannot change password at this time.";
    public const string ChangePasswordSuccess = "Your password was changed successfully.";
    public const string DeactivateAccountFailed = "Your account cannot be deactivated at this time";
    public const string DeactivateAccountSuccess = "Your account was deactivated successfully";
    public const string LoginFailedGeneric = "Login failed. Please check your credentials.";
    public const string LoginFailedAccountLocked = "Account might be locked out. Please retry in 24 hours";
    public const string SignUpFailed = "Sign up failed. Please review errors and try again.";
    public const string SignUpSuccess = "Sign up successful.";
    public const string TokenRefreshFailed = "Token refresh failed.";
}
public class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}