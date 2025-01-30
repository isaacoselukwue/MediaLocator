global using Microsoft.AspNetCore.Identity;
using ML.Domain.Common;

namespace ML.Domain.Entities;
public class Users : IdentityUser<Guid>
{
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
