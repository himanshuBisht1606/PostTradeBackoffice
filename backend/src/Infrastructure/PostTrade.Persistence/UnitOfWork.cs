using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PostTrade.Application.Interfaces;
using PostTrade.Persistence.Context;

namespace PostTrade.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostTradeDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(PostTradeDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void ClearTracking() => _context.ChangeTracker.Clear();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class
    {
        return await _context.Set<T>().Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<long[]> GetNextSequenceValuesAsync(string sequenceName, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0) return Array.Empty<long>();
        // Single round-trip to fetch N sequence values — equivalent of Oracle SYSDBSEQUENCE bulk fetch
        return await _context.Database
            .SqlQueryRaw<long>($"SELECT nextval('{sequenceName}') FROM generate_series(1, {count})")
            .ToArrayAsync(cancellationToken);
    }
}
