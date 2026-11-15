using Blogify.Domain.Abstractions;
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

    public async Task<PagedList<Tag>> GetPagedAsync(int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await DbContext.Set<Tag>().CountAsync(cancellationToken);
        var items = await DbContext.Set<Tag>()
            .AsNoTracking()
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Tag>(items, total);
    }

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return [];

        return await DbContext
            .Set<Tag>()
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);
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