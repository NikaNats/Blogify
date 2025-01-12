using Blogify.Application.Categories.DeleteCategory;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.Delete;

public class DeleteCategoryCommandHandlerTests
{
    private readonly DeleteCategoryCommandHandler _handler;
    private readonly ICategoryRepository _categoryRepositoryMock;

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
        Result result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        Result result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
        result.Error.Description.Should().Be("Category not found.");
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
        Result result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.UnexpectedError);
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