using Blogify.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CategoryRepository(ApplicationDbContext dbContext)
    : Repository<Category>(dbContext), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllWithPostsCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .AsNoTracking()
            .Include(c => c.Posts)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Category>()
            .FirstOrDefaultAsync(c => c.Name.Value == name, cancellationToken);
    }

    // public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    // {
    //     return await DbContext
    //         .Set<Category>()
    //         .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    // }
    //
    // public async Task<IReadOnlyCollection<Category?>> GetAllAsync(CancellationToken cancellationToken = default)
    // {
    //     return await DbContext
    //         .Set<Category>()
    //         .ToListAsync(cancellationToken);
    // }
    //
    // public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    // {
    //     await DbContext.Set<Category>().AddAsync(category, cancellationToken);
    //     await DbContext.SaveChangesAsync(cancellationToken);
    // }
    //
    // public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    // {
    //     DbContext.Set<Category>().Update(category);
    //     await DbContext.SaveChangesAsync(cancellationToken);
    // }
    // public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    // {
    //     DbContext.Set<Category>().Remove(category); 
    //     await DbContext.SaveChangesAsync(cancellationToken);
    // }
}