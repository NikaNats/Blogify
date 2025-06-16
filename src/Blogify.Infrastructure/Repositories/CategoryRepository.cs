using System.Linq.Expressions;
using Blogify.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Category>().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Category>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Category>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Category>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Category>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Category>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Category>().FirstOrDefaultAsync(c => c.Name.Value == name, cancellationToken);
    }
}