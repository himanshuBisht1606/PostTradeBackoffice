using System.Linq.Expressions;

namespace PostTrade.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void ClearTracking();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Fetches <paramref name="count"/> values from a PostgreSQL sequence in a single round-trip.
    /// Equivalent of Oracle SYSDBSEQUENCE.NEXTVAL bulk fetch used in CFORise bulk loader.
    /// </summary>
    Task<long[]> GetNextSequenceValuesAsync(string sequenceName, int count, CancellationToken cancellationToken = default);
}
