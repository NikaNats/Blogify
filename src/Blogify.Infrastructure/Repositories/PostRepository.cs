using System.Linq.Expressions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class PostRepository(ApplicationDbContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // A single post for a command or detail view needs its full state, including comments.
        return await dbContext.Set<Post>()
            .Include(p => p.Comments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveCategoryReferencesAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE posts SET category_ids = array_remove(category_ids, {categoryId}) WHERE {categoryId} = ANY(category_ids)",
            cancellationToken);
    }

    public async Task RemoveTagReferencesAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE posts SET tag_ids = array_remove(tag_ids, {tagId}) WHERE {tagId} = ANY(tag_ids)",
            cancellationToken);
    }

    public Task AddAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<PostVisibility?> GetVisibilityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PostVisibility(p.Status, p.AuthorId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<PagedList<Post>> GetPagedAsync(int page, int pageSize, bool publishedOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await ToPagedAsync(Posts(publishedOnly), page, pageSize, cancellationToken);
    }

    public async Task<PagedList<Post>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize,
        bool publishedOnly = true, CancellationToken cancellationToken = default)
    {
        return await ToPagedAsync(
            Posts(publishedOnly).Where(post => post.AuthorId == authorId),
            page, pageSize, cancellationToken);
    }

    public async Task<PagedList<Post>> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize,
        bool publishedOnly = true, CancellationToken cancellationToken = default)
    {
        return await ToPagedAsync(
            Posts(publishedOnly).Where(post => post.CategoryIds.Contains(categoryId)),
            page, pageSize, cancellationToken);
    }

    public async Task<PagedList<Post>> GetByTagIdAsync(Guid tagId, int page, int pageSize,
        bool publishedOnly = true, CancellationToken cancellationToken = default)
    {
        return await ToPagedAsync(
            Posts(publishedOnly).Where(post => post.TagIds.Contains(tagId)),
            page, pageSize, cancellationToken);
    }

    // The status filter is applied in SQL so counts and paging only ever see visible posts.
    private IQueryable<Post> Posts(bool publishedOnly)
    {
        var query = dbContext.Set<Post>().AsNoTracking();
        return publishedOnly ? query.Where(post => post.Status == PublicationStatus.Published) : query;
    }

    private static async Task<PagedList<Post>> ToPagedAsync(IQueryable<Post> query, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Post>(items, total);
    }
}
