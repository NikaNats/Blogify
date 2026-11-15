using Blogify.Application.Abstractions.Data;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Dapper;

namespace Blogify.Application.Categories.GetAllCategoriesWithPostCount;

internal sealed class GetAllCategoriesWithPostCountQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetAllCategoriesWithPostCountQuery, List<CategoryWithPostCountResponse>>
{
    public async Task<Result<List<CategoryWithPostCountResponse>>> Handle(
        GetAllCategoriesWithPostCountQuery request,
        CancellationToken cancellationToken)
    {
        // This handler now completely bypasses the repositories and domain entities for reading.
        // It executes a raw, optimized SQL query for maximum performance.
        using var connection = sqlConnectionFactory.CreateConnection();

        // This SQL is an example. It efficiently gets the category and its post count in one go.
        const string sql = """
                           SELECT
                               c.id AS Id,
                               c.name AS Name,
                               c.description AS Description,
                               (SELECT COUNT(*) FROM posts p WHERE c.id = ANY(p.category_ids)) AS PostCount
                           FROM categories c
                           ORDER BY c.name
                           """;

        var categories = await connection.QueryAsync<CategoryWithPostCountResponse>(sql);

        return Result.Success(categories.ToList());
    }
}