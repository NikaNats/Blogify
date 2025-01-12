using System.Data;
using Blogify.Application.Abstractions.Data;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Users;
using Bogus;
using Dapper;

namespace Blogify.Api.Extensions;

internal static class SeedDataExtensions
{
    private const string DisableDataSeedingKey = "DisableDataSeeding";
    private const int DefaultNumberOfCategories = 10;
    private const int DefaultNumberOfTags = 10;
    private const int DefaultNumberOfUsers = 10;
    private const int DefaultNumberOfPosts = 20;
    private const int DefaultNumberOfComments = 50;

    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var configuration = services.GetRequiredService<IConfiguration>();

        if (configuration.GetValue<bool>(DisableDataSeedingKey))
        {
            return;
        }

        var sqlConnectionFactory = services.GetRequiredService<ISqlConnectionFactory>();
        using var connection = sqlConnectionFactory.CreateConnection();

        var faker = new Faker();

        await SeedDataInternalAsync(connection, faker);
    }

    private static async Task SeedDataInternalAsync(IDbConnection connection, Faker faker)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            var categoryIds = await SeedCategoriesAsync(connection, transaction, faker);
            var tagIds = await SeedTagsAsync(connection, transaction, faker);
            var userIds = await SeedUsersAsync(connection, transaction, faker);
            var postIds = await SeedPostsAsync(connection, transaction, faker, userIds);
            await SeedCommentsAsync(connection, transaction, faker, postIds, userIds);

            await SeedPostCategoriesAsync(connection, transaction, postIds, categoryIds, faker);
            await SeedPostTagsAsync(connection, transaction, postIds, tagIds, faker);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new ApplicationException("An error occurred while seeding data.", ex);
        }
    }

    private static async Task<List<Guid>> SeedCategoriesAsync(IDbConnection connection, IDbTransaction transaction, Faker faker)
    {
        var categories = GenerateEntities(faker, DefaultNumberOfCategories, () => Category.Create(
            faker.Commerce.Department(),
            faker.Commerce.ProductDescription()
        ).Value);

        const string sql = @"
    INSERT INTO categories (id, name, description, created_at, last_modified_at)
    VALUES (@Id, @Name, @Description, @CreatedAt, @LastModifiedAt)
    ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, categories.Select(c => new
        {
            c.Id,
            Name = c.Name.Value,
            Description = c.Description.Value,
            CreatedAt = c.CreatedAt,
            LastModifiedAt = (DateTime?)null
        }), transaction);

        return categories.Select(c => c.Id).ToList();
    }

    private static async Task<List<Guid>> SeedTagsAsync(IDbConnection connection, IDbTransaction transaction, Faker faker)
    {
        var tags = GenerateEntities(faker, DefaultNumberOfTags, () => Tag.Create(
            faker.Lorem.Word()
        ).Value);

        const string sql = @"
    INSERT INTO tags (id, name, created_at, last_modified_at)
    VALUES (@Id, @Name, @CreatedAt, @LastModifiedAt)
    ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, tags.Select(t => new
        {
            t.Id,
            Name = t.Name.Value,
            CreatedAt = t.CreatedAt,
            LastModifiedAt = (DateTime?)null
        }), transaction);

        return tags.Select(t => t.Id).ToList();
    }

    private static async Task<List<Guid>> SeedUsersAsync(IDbConnection connection, IDbTransaction transaction, Faker faker)
    {
        var users = GenerateEntities(faker, DefaultNumberOfUsers, () => User.Create(
            FirstName.Create(faker.Name.FirstName()).Value,
            LastName.Create(faker.Name.LastName()).Value,
            Email.Create(faker.Internet.Email()).Value
        ).Value);

        const string sql = @"
        INSERT INTO users (id, first_name, last_name, email, identity_id)
        VALUES (@Id, @FirstName, @LastName, @Email, @IdentityId)
        ON CONFLICT (identity_id) DO NOTHING;";

        await connection.ExecuteAsync(sql, users.Select(u => new
        {
            u.Id,
            FirstName = u.FirstName.Value,
            LastName = u.LastName.Value,
            Email = u.Email.Address,
            u.IdentityId,
        }), transaction);

        return users.Select(u => u.Id).ToList();
    }

    private static async Task<List<Guid>> SeedPostsAsync(IDbConnection connection, IDbTransaction transaction, Faker faker, List<Guid> userIds)
    {
        var posts = GenerateEntities(faker, DefaultNumberOfPosts, () => Post.Create(
            PostTitle.Create(faker.Lorem.Sentence(5)).Value,
            PostContent.Create(faker.Lorem.Paragraphs(3)).Value,
            PostExcerpt.Create(faker.Lorem.Sentence(10)).Value,
            faker.PickRandom(userIds)
        ).Value);

        const string sql = @"
        INSERT INTO posts (id, title, content, excerpt, slug, author_id, created_at, last_modified_at, published_at, status)
        VALUES (@Id, @Title, @Content, @Excerpt, @Slug, @AuthorId, @CreatedAt, @LastModifiedAt, @PublishedAt, @Status)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, posts.Select(p => new
        {
            p.Id,
            Title = p.Title.Value,
            Content = p.Content.Value,
            Excerpt = p.Excerpt.Value,
            Slug = p.Title.Value,
            AuthorId = p.AuthorId,
            CreatedAt = p.CreatedAt,
            LastModifiedAt = (DateTime?)null,
            PublishedAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow),
            Status = "Published"
        }), transaction);

        return posts.Select(p => p.Id).ToList();
    }

    private static async Task SeedCommentsAsync(IDbConnection connection, IDbTransaction transaction, Faker faker, List<Guid> postIds, List<Guid> userIds)
    {
        var comments = GenerateEntities(faker, DefaultNumberOfComments, () => Comment.Create(
            faker.Lorem.Sentence(faker.Random.Int(5, 15)),
            faker.PickRandom(userIds),
            faker.PickRandom(postIds)
        ).Value);

        const string sql = @"
        INSERT INTO comments (id, content_value, author_id, post_id, created_at, last_modified_at)
        VALUES (@Id, @Content, @AuthorId, @PostId, @CreatedAt, @LastModifiedAt)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, comments.Select(c => new
        {
            c.Id,
            Content = c.Content.Value, // Ensure this is a simple type (e.g., string)
            AuthorId = c.AuthorId,
            PostId = c.PostId,
            CreatedAt = c.CreatedAt,
            LastModifiedAt = (DateTime?)null
        }), transaction);
    }

    private static async Task SeedPostCategoriesAsync(IDbConnection connection, IDbTransaction transaction, List<Guid> postIds, List<Guid> categoryIds, Faker faker)
    {
        var postCategories = postIds
            .SelectMany(postId => categoryIds
                .OrderBy(_ => Guid.NewGuid())
                .Take(faker.Random.Int(1, 3))
                .Select(categoryId => new 
                { 
                    post_id = postId, 
                    category_id = categoryId 
                }))
            .ToList();

        const string sql = @"
    INSERT INTO blog.postcategories (post_id, category_id)
    VALUES (@post_id, @category_id)
    ON CONFLICT (post_id, category_id) DO NOTHING;";

        await connection.ExecuteAsync(sql, postCategories, transaction);
    }

    private static async Task SeedPostTagsAsync(IDbConnection connection, IDbTransaction transaction, List<Guid> postIds, List<Guid> tagIds, Faker faker)
    {
        var postTags = postIds
            .SelectMany(postId => tagIds
                .OrderBy(_ => Guid.NewGuid()) // Randomize tag selection
                .Take(faker.Random.Int(1, 3)) // Assign 1-3 tags per post
                .Select(tagId => new 
                { 
                    post_id = postId, 
                    tag_id = tagId,
                }))
            .ToList();

        const string sql = @"
    INSERT INTO blog.posttags (post_id, tag_id)
    VALUES (@post_id, @tag_id)
    ON CONFLICT (post_id, tag_id) DO NOTHING;";

        await connection.ExecuteAsync(sql, postTags, transaction);
    }

    private static List<T> GenerateEntities<T>(Faker faker, int count, Func<T> entityGenerator)
    {
        return Enumerable.Range(1, count)
            .Select(_ => entityGenerator())
            .ToList();
    }
}