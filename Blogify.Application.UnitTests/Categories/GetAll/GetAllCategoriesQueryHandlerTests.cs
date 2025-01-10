using Blogify.Application.Categories.GetAllCategories;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.GetAll;

public class GetAllCategoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllCategoriesMappedToResponse()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetAllCategoriesQueryHandler(categoryRepository);
        var categories = new List<Category>
        {
            Category.Create("Category1", "Description1").Value,
            Category.Create("Category2", "Description2").Value
        };
        categoryRepository.GetAllAsync(CancellationToken.None).Returns(categories);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Id.ShouldBe(categories[0].Id);
        result.Value[0].Name.ShouldBe(categories[0].Name.Value);
        result.Value[0].Description.ShouldBe(categories[0].Description.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCategories()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetAllCategoriesQueryHandler(categoryRepository);
        categoryRepository.GetAllAsync(CancellationToken.None).Returns(new List<Category>());

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new GetAllCategoriesQueryHandler(categoryRepository);
        categoryRepository.GetAllAsync(CancellationToken.None).Throws(new Exception("Repository failed"));

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }
}