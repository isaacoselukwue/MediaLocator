using System.ComponentModel.DataAnnotations;

namespace ML.Web.Models;

public class PasswordResetModel
{
    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$",
        ErrorMessage = "Password must be at least 12 characters and include uppercase letter, lowercase letter, number, and special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class PasswordResetRequest
{
    public string? UserId { get; set; }
    public string? ResetToken { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}