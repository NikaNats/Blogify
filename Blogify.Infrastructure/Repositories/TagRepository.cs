using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class TagRepository(ApplicationDbContext dbContext) : Repository<Tag>(dbContext), ITagRepository
{
    public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Tag>()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<Tag>().AddAsync(tag, cancellationToken);
    }

    public Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        DbContext.Set<Tag>().Update(tag);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await DbContext
            .Set<Tag>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag != null) DbContext.Set<Tag>().Remove(tag);
    }
}