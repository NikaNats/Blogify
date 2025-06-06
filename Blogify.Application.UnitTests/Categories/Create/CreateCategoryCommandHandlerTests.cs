using Blogify.Application.Categories.CreateCategory;
using Blogify.Domain.Categories;
using Shouldly;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.Create;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCategoryIsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand("Category1", "Description1");

        _categoryRepositoryMock
            .AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ToString().ShouldNotBeNullOrEmpty();
        await _categoryRepositoryMock.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenNameIsInvalid()
    {
        // Arrange
        var command = new CreateCategoryCommand("", "Description1"); // Empty name is invalid

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NameNullOrEmpty");
        result.Error.Description.ShouldBe("Category name cannot be null or empty.");
        await _categoryRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenDescriptionIsInvalid()
    {
        // Arrange
        var command = new CreateCategoryCommand("Category1", ""); // Empty description is invalid

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.DescriptionNullOrEmpty");
        result.Error.Description.ShouldBe("Category description cannot be null or empty.");
        await _categoryRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        var command = new CreateCategoryCommand("Category1", "Description1");

        _categoryRepositoryMock
            .AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.UnexpectedError);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryAdd_WhenCategoryIsValid()
    {
        // Arrange
        var command = new CreateCategoryCommand("Category1", "Description1");

        _categoryRepositoryMock
            .AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, default);

        // Assert
        await _categoryRepositoryMock.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }
}
