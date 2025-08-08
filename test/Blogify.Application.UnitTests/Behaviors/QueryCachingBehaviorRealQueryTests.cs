using Blogify.Application.Abstractions.Behaviors;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Behaviors;

public class QueryCachingBehaviorRealQueryTests
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingBehavior<GetAllCategoriesQuery, Result<List<AllCategoryResponse>>>> _logger;
    private readonly QueryCachingBehavior<GetAllCategoriesQuery, Result<List<AllCategoryResponse>>> _behavior;
    private readonly RequestHandlerDelegate<Result<List<AllCategoryResponse>>> _next;

    public QueryCachingBehaviorRealQueryTests()
    {
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<QueryCachingBehavior<GetAllCategoriesQuery, Result<List<AllCategoryResponse>>>>>();
        _behavior = new QueryCachingBehavior<GetAllCategoriesQuery, Result<List<AllCategoryResponse>>>(_cacheService, _logger);
        _next = Substitute.For<RequestHandlerDelegate<Result<List<AllCategoryResponse>>>>();
    }

    [Fact]
    public async Task Handle_CacheMiss_Should_InvokeHandler_And_SetCache()
    {
        // Arrange
        var query = new GetAllCategoriesQuery();
        _cacheService.GetAsync<Result<List<AllCategoryResponse>>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns((Result<List<AllCategoryResponse>>?)null); // miss

        var category = Category.Create("Cat1", "Desc").Value;
        var response = Result.Success(new List<AllCategoryResponse>
        {
            new(category.Id, category.Name.Value, category.Description.Value, category.CreatedAt, category.LastModifiedAt)
        });

        _next.Invoke().Returns(response);

        // Act
        var result = await _behavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _next.Received(1).Invoke();
        await _cacheService.Received(1).SetAsync(query.CacheKey, response, query.Expiration, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CacheHit_Should_Not_InvokeHandler()
    {
        // Arrange
        var query = new GetAllCategoriesQuery();
        var category = Category.Create("Cat2", "Desc2").Value;
        var cached = Result.Success(new List<AllCategoryResponse>
        {
            new(category.Id, category.Name.Value, category.Description.Value, category.CreatedAt, category.LastModifiedAt)
        });

        _cacheService.GetAsync<Result<List<AllCategoryResponse>>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns(cached); // hit

        // Act
        var result = await _behavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.ShouldBe(cached);
        await _next.DidNotReceive().Invoke();
        await _cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<Result<List<AllCategoryResponse>>>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HandlerFailure_Should_Not_SetCache()
    {
        // Arrange
        var query = new GetAllCategoriesQuery();
        _cacheService.GetAsync<Result<List<AllCategoryResponse>>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns((Result<List<AllCategoryResponse>>?)null); // miss

        var error = new Error("Categories.Failure", "Failed", ErrorType.Failure);
        var failure = Result.Failure<List<AllCategoryResponse>>(error);
        _next.Invoke().Returns(failure);

        // Act
        var result = await _behavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        await _cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<Result<List<AllCategoryResponse>>>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }
}
