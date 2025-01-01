using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Tag>()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<Tag>().AddAsync(tag, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public override void Add(Tag entity)
    {
        base.Add(entity);
    }
}