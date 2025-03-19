using System.ComponentModel.DataAnnotations;

namespace ML.Web.Models;

public class PasswordChangeModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$",
        ErrorMessage = "Password must be at least 12 characters and include uppercase letter, lowercase letter, number, and special character")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public StatusEnum StatusEnum { get; set; }
}

public class ChangePasswordRequest
{
    public string? NewPassword { get; set; }
    public string? ConfirmNewPassword { get; set; }
}

public class ChangeUserRoleRequest
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
}

public class ActivateAccountRequest
{
    public Guid UserId { get; set; }
}

public class DeleteAccountRequest
{
    public Guid UserId { get; set; }
    public bool IsPermanant { get; set; }
}

public class UserAccountResult
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset DateAccountCreated { get; set; }
    public StatusEnum Status { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
public class PaginatedUserAccountDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public List<UserAccountResult> Results { get; set; } = [];
}
public enum StatusEnum
{
    Pending = 0,
    Active = 1,
    Deleted = 2,
    InActive = 3
}


public class DeactivateAccountAdminRequest
{
    public Guid UserId { get; set; }
}