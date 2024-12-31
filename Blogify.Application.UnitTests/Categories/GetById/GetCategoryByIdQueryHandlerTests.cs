using Blogify.Application.Categories.GetCategoryById;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.GetById;

public class GetCategoryByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategoryResponse_WhenCategoryExists()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetCategoryByIdQueryHandler(categoryRepository);
        var categoryId = Guid.NewGuid();

        // Create a category with the same Id as the one being queried
        var category = Category.Create("TestCategory", "Test Description").Value;
        category.GetType().GetProperty("Id")?.SetValue(category, categoryId); // Set the Id explicitly

        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(new GetCategoryByIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(categoryId);
        result.Value.Name.ShouldBe("TestCategory");
        result.Value.Description.ShouldBe("Test Description");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetCategoryByIdQueryHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns((Category)null);

        // Act
        var result = await handler.Handle(new GetCategoryByIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetCategoryByIdQueryHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Throws(new Exception("Repository failed"));

        // Act
        var result = await handler.Handle(new GetCategoryByIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }
}