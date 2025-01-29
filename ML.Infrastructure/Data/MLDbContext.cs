global using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using ML.Domain.Entities;

namespace ML.Infrastructure.Data;
public class MLDbContext : IdentityDbContext<Users, UserRoles, Guid>, IDataProtectionKeyContext
{
    public MLDbContext(DbContextOptions<MLDbContext> options) : base(options)
    {
    }
    public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
