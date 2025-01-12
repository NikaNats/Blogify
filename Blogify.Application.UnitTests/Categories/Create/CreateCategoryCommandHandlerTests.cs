using Blogify.Application.Categories.CreateCategory;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.Create;

public class CreateCategoryCommandHandlerTests
{
    private readonly CreateCategoryCommandHandler _handler;
    private readonly ICategoryRepository _categoryRepositoryMock;

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
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _categoryRepositoryMock.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenNameIsInvalid()
    {
        // Arrange
        var command = new CreateCategoryCommand("", "Description1"); // Empty name is invalid

        // Act
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameNullOrEmpty");
        result.Error.Description.Should().Be("Category name cannot be null or empty.");
        await _categoryRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenDescriptionIsInvalid()
    {
        // Arrange
        var command = new CreateCategoryCommand("Category1", ""); // Empty description is invalid

        // Act
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.DescriptionNullOrEmpty");
        result.Error.Description.Should().Be("Category description cannot be null or empty.");
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
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.UnexpectedError);
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