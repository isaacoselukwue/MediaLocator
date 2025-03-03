using ML.Domain.Common;

namespace ML.Domain.Entities;
public class PasswordHistories : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Users? User { get; set; }
    public string? PasswordHash { get; set; }
}
