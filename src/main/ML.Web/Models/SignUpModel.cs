using System.ComponentModel.DataAnnotations;

namespace ML.Web.Models;
public class SignupModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$",
        ErrorMessage = "Password must be at least 12 characters and include uppercase letter, lowercase letter, number, and special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[0]\d{6,13}$", ErrorMessage = "Phone number must start with '0' and contain 7-14 digits")]
    public string PhoneNumber { get; set; } = string.Empty;
}