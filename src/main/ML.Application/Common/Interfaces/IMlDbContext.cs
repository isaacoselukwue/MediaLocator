namespace ML.Application.Common.Interfaces;
public interface IMlDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
