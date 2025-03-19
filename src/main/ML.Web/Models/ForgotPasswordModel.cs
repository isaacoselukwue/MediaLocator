using System.ComponentModel.DataAnnotations;

namespace ML.Web.Models;

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}

public class InitiatePasswordResetRequest
{
    public string? EmailAddress { get; set; }
}