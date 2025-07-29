using System.Linq.Expressions;
using Blogify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal abstract class Repository<TEntity>(ApplicationDbContext dbContext)
    : IRepository<TEntity> where TEntity : Entity
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();
    protected readonly ApplicationDbContext DbContext = dbContext;

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    protected virtual IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}