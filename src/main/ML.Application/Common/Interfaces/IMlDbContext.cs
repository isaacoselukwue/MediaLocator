using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ML.Domain.Entities;

namespace ML.Application.Common.Interfaces;
public interface IMLDbContext
{
    DbSet<PasswordHistories> PasswordHistories { get; set; }
    DbSet<UserRoles> Roles { get; set; }
    DbSet<SearchHistories> SearchHistories { get; set; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; set; }
    DbSet<Users> Users { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
