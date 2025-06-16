using Blogify.Application.Categories.GetCategoryById;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.GetById;

public class GetCategoryByIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _handler = new GetCategoryByIdQueryHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldReturnSuccessWithCorrectData()
    {
        // Arrange
        var category = TestFactory.CreateCategory();
        var query = new GetCategoryByIdQuery(category.Id);

        _categoryRepositoryMock.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.Id.ShouldBe(category.Id);
        response.Name.ShouldBe(category.Name.Value);
        response.Description.ShouldBe(category.Description.Value);
        response.CreatedAt.ShouldBe(category.CreatedAt);
        response.UpdatedAt.ShouldBe(category.LastModifiedAt);
    }

    [Fact]
    public async Task Handle_WhenCategoryIsNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.NewGuid());
        _categoryRepositoryMock.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.NewGuid());
        var expectedException = new Exception("Database connection failed");

        _categoryRepositoryMock.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        // We assert that calling the handler will throw the exact exception we configured the mock to throw.
        // This proves the handler is not catching it, allowing the pipeline to do so.
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, default));

        exception.ShouldBe(expectedException);
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Category CreateCategory()
        {
            var result = Category.Create(
                "Test Category",
                "A description for testing purposes.");

            result.IsSuccess.ShouldBeTrue("Test setup failed: could not create a valid Category.");
            return result.Value;
        }
    }

    #endregion
}