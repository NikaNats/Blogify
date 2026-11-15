using Blogify.Application.Categories.GetAllCategories;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.GetAll;

public class GetAllCategoriesTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly GetAllCategoriesQueryHandler _handler;

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
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(TestPaging.Of(new List<Category>().AsReadOnly()));

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnListOfCategories_WhenCategoriesExist()
    {
        // Arrange
        var category1 = Category.Create("Category1", "Description1").Value;
        var category2 = Category.Create("Category2", "Description2").Value;
        var categories = new List<Category> { category1, category2 }.AsReadOnly();

        _categoryRepositoryMock
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(TestPaging.Of(categories));

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.Items[0].Name.ShouldBe("Category1");
        result.Value.Items[1].Name.ShouldBe("Category2");
    }

    [Fact]
    public async Task Handle_Should_PropagateException_WhenRepositoryThrows()
    {
        // Arrange: infrastructure failures are not domain failures; the global exception handler owns them.
        _categoryRepositoryMock
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetAllCategoriesQuery();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _handler.Handle(query, default));
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryOnce()
    {
        // Arrange
        var category = Category.Create("Category1", "Description1").Value;
        var categories = new List<Category> { category }.AsReadOnly();

        _categoryRepositoryMock
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(TestPaging.Of(categories));

        var query = new GetAllCategoriesQuery();

        // Act
        await _handler.Handle(query, default);

        // Assert
        await _categoryRepositoryMock.Received(1).GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}