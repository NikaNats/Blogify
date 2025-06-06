using Blogify.Application.Categories.DeleteCategory;
using Blogify.Domain.Categories;
using Shouldly;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.Delete;

public class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Category1", "Description1").Value;

        _categoryRepositoryMock
            .GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        var command = new DeleteCategoryCommand(categoryId);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _categoryRepositoryMock.Received(1).DeleteAsync(category, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var command = new DeleteCategoryCommand(categoryId);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NotFound");
        result.Error.Description.ShouldBe("Category not found.");
        await _categoryRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Category1", "Description1").Value;

        _categoryRepositoryMock
            .GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        _categoryRepositoryMock
            .DeleteAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        var command = new DeleteCategoryCommand(categoryId);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.UnexpectedError);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryDelete_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Category1", "Description1").Value;

        _categoryRepositoryMock
            .GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        var command = new DeleteCategoryCommand(categoryId);

        // Act
        await _handler.Handle(command, default);

        // Assert
        await _categoryRepositoryMock.Received(1).DeleteAsync(category, Arg.Any<CancellationToken>());
    }
}
