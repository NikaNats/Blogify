using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class TagRepository(ApplicationDbContext dbContext) : Repository<Tag>(dbContext), ITagRepository
{
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await DbContext
            .Set<Tag>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag != null) DbContext.Set<Tag>().Remove(tag);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name);
        
        return await DbContext
            .Set<Tag>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name.Value == name, cancellationToken);
    }
}