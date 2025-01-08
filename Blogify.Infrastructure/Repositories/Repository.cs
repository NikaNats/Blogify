using System.Linq.Expressions;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal abstract class Repository<TEntity>(ApplicationDbContext dbContext)
    : IRepository<TEntity> where TEntity : Entity
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();
    protected readonly ApplicationDbContext DbContext = dbContext;

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual void Add(TEntity entity)
    {
        DbContext.Add(entity);
    }
}