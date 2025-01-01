using Blogify.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<Category>().AddAsync(category, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        DbContext.Set<Category>().Update(category);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public void Update(Category category)
    {
        DbContext.Set<Category>().Update(category);
    }
}