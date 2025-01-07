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
            var postIds = await SeedPostsAsync(connection, transaction, faker, categoryIds, userIds);
            await SeedCommentsAsync(connection, transaction, faker, postIds, userIds);

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
        INSERT INTO ""Categories"" (id, name, description, created_at, updated_at)
        VALUES (@Id, @Name, @Description, @CreatedAt, @UpdatedAt)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            CreatedAt = c.CreatedAt,
            UpdatedAt = (DateTime?)null
        }), transaction);

        return categories.Select(c => c.Id).ToList();
    }

    private static async Task<List<Guid>> SeedTagsAsync(IDbConnection connection, IDbTransaction transaction, Faker faker)
    {
        var tags = GenerateEntities(faker, DefaultNumberOfTags, () => Tag.Create(
            faker.Lorem.Word()
        ).Value);

        const string sql = @"
        INSERT INTO ""Tags"" (id, name, created_at)
        VALUES (@Id, @Name, @CreatedAt)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, tags, transaction);

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
            IdentityId = u.IdentityId
        }), transaction);

        return users.Select(u => u.Id).ToList();
    }

    private static async Task<List<Guid>> SeedPostsAsync(IDbConnection connection, IDbTransaction transaction, Faker faker, List<Guid> categoryIds, List<Guid> userIds)
    {
        var posts = GenerateEntities(faker, DefaultNumberOfPosts, () => Post.Create(
            PostTitle.Create(faker.Lorem.Sentence(5)).Value,
            PostContent.Create(faker.Lorem.Paragraphs(3)).Value,
            PostExcerpt.Create(faker.Lorem.Sentence(10)).Value,
            faker.PickRandom(userIds),
            faker.PickRandom(categoryIds)
        ).Value);

        const string sql = @"
        INSERT INTO ""Posts"" (id, title_value, content_value, excerpt_value, slug_value, author_id, category_id, created_at, updated_at, published_at, status)
        VALUES (@Id, @TitleValue, @ContentValue, @ExcerptValue, @SlugValue, @AuthorId, @CategoryId, @CreatedAt, @UpdatedAt, @PublishedAt, @Status)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, posts.Select(p => new
        {
            p.Id,
            TitleValue = p.Title.Value,
            ContentValue = p.Content.Value,
            ExcerptValue = p.Excerpt.Value,
            SlugValue = p.Title.Value,
            AuthorId = p.AuthorId,
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = (DateTime?)null,
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
        INSERT INTO ""Comments"" (id, content, author_id, post_id, created_at)
        VALUES (@Id, @Content, @AuthorId, @PostId, @CreatedAt)
        ON CONFLICT (id) DO NOTHING;";

        await connection.ExecuteAsync(sql, comments.Select(c => new
        {
            c.Id,
            c.Content,
            c.AuthorId,
            c.PostId,
            c.CreatedAt
        }), transaction);
    }

    private static List<T> GenerateEntities<T>(Faker faker, int count, Func<T> entityGenerator)
    {
        return Enumerable.Range(1, count)
            .Select(_ => entityGenerator())
            .ToList();
    }
}