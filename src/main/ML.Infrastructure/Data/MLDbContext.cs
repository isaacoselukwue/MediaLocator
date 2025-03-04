global using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using ML.Domain.Entities;
using System.Reflection;

namespace ML.Infrastructure.Data;
public class MLDbContext : IdentityDbContext<Users, UserRoles, Guid>, IDataProtectionKeyContext, IMLDbContext
{
    public MLDbContext(DbContextOptions<MLDbContext> options) : base(options)
    {
    }
    public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public virtual DbSet<PasswordHistories> PasswordHistories { get; set; }
    public virtual DbSet<SearchHistories> SearchHistories { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Users>().ToTable("Users");
        builder.Entity<UserRoles>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
