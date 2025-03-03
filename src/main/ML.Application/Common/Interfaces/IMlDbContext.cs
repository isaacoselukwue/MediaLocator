using Microsoft.EntityFrameworkCore;
using ML.Domain.Entities;

namespace ML.Application.Common.Interfaces;
public interface IMLDbContext
{
    DbSet<PasswordHistories> PasswordHistories { get; set; }
    DbSet<SearchHistories> SearchHistories { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
