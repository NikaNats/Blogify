using Blogify.Application.Abstractions.Data;
using Dapper;

namespace Blogify.FunctionalTests.Infrastructure;

/// <summary>
///     Provides a clean and direct way to seed test data into the database for functional tests.
///     This bypasses the API, ensuring tests are fast, reliable, and independent of the application's business logic.
/// </summary>
public class BlogifyTestSeeder(ISqlConnectionFactory sqlConnectionFactory)
{
    public async Task<Guid> SeedPostAsync(Guid? authorId = null, string status = "Published")
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        var postId = Guid.NewGuid();
        var categoryId = await SeedCategoryAsync(); // Seed a category
        var tagId = await SeedTagAsync(); // Seed a tag
        const string sql = """
                           INSERT INTO posts (id, title, content, excerpt, slug, author_id, created_at, published_at, status, category_ids, tag_ids)
                           VALUES (@Id, @Title, @Content, @Excerpt, @Slug, @AuthorId, @CreatedAt, @PublishedAt, @Status, @CategoryIds, @TagIds)
                           """;

        await connection.ExecuteAsync(sql, new
        {
            Id = postId,
            Title = $"Test Post {postId}",
            Content =
                "This is a valid post content that is long enough to pass any validation checks that might exist in the system.",
            Excerpt = "A test excerpt.",
            Slug = $"test-post-{postId}",
            AuthorId = authorId ?? Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            PublishedAt = status == "Published" ? DateTimeOffset.UtcNow : (DateTimeOffset?)null,
            Status = status,
            CategoryIds = new[] { categoryId }, // Use the seeded category ID
            TagIds = new[] { tagId } // Use the seeded tag ID
        });

        return postId;
    }

    public async Task<Guid> SeedCommentAsync(Guid postId, Guid? authorId = null,
        string content = "Default test comment")
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        var commentId = Guid.NewGuid();
        const string sql = """
                           INSERT INTO comments (id, content_value, author_id, post_id, created_at)
                           VALUES (@Id, @Content, @AuthorId, @PostId, @CreatedAt)
                           """;

        await connection.ExecuteAsync(sql, new
        {
            Id = commentId,
            Content = $"{content} {commentId}",
            AuthorId = authorId ?? Guid.NewGuid(),
            PostId = postId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        return commentId;
    }

    public async Task<Guid> SeedCategoryAsync(string? name = null, string? description = null)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        var categoryId = Guid.NewGuid();
        const string sql = """
                           INSERT INTO categories (id, name, description, created_at)
                           VALUES (@Id, @Name, @Description, @CreatedAt)
                           """;

        await connection.ExecuteAsync(sql, new
        {
            Id = categoryId,
            Name = name ?? $"Test Category {categoryId}",
            Description = description ?? "A test category description.",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return categoryId;
    }

    public async Task<Guid> SeedTagAsync(string? name = null)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        var tagId = Guid.NewGuid();
        const string sql = """
                           INSERT INTO tags (id, name, created_at)
                           VALUES (@Id, @Name, @CreatedAt)
                           """;

        await connection.ExecuteAsync(sql, new
        {
            Id = tagId,
            Name = name ?? $"Test Tag {tagId}",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return tagId;
    }
}