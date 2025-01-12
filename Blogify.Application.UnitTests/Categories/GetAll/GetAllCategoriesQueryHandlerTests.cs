using Blogify.Application.Categories.GetAllCategories;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.GetAll;

public class GetAllCategoriesTests
{
    private readonly GetAllCategoriesQueryHandler _handler;
    private readonly ICategoryRepository _categoryRepositoryMock;

    public GetAllCategoriesTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _handler = new GetAllCategoriesQueryHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        _categoryRepositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>().AsReadOnly());

        var query = new GetAllCategoriesQuery();

        // Act
        Result<List<AllCategoryResponse>> result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnListOfCategories_WhenCategoriesExist()
    {
        // Arrange
        var category1 = Category.Create("Category1", "Description1").Value;
        var category2 = Category.Create("Category2", "Description2").Value;

        var categories = new List<Category> { category1, category2 }.AsReadOnly();

        _categoryRepositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories);

        var query = new GetAllCategoriesQuery();

        // Act
        Result<List<AllCategoryResponse>> result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Category1");
        result.Value[1].Name.Should().Be("Category2");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        _categoryRepositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetAllCategoriesQuery();

        // Act
        Result<List<AllCategoryResponse>> result = await _handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.UnexpectedError);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryOnce()
    {
        // Arrange
        var category = Category.Create("Category1", "Description1").Value;

        var categories = new List<Category> { category }.AsReadOnly();

        _categoryRepositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories);

        var query = new GetAllCategoriesQuery();

        // Act
        await _handler.Handle(query, default);

        // Assert
        await _categoryRepositoryMock.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }
}