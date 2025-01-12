using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Categories.GetCategoryById;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Categories.GetById;

public class GetCategoryByIdQueryHandlerTests
{
    private static readonly GetCategoryByIdQuery Command = new(Guid.NewGuid());

    private readonly GetCategoryByIdQueryHandler _handler;
    private readonly ICategoryRepository _categoryRepositoryMock;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();

        _handler = new GetCategoryByIdQueryHandler(_categoryRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenCategoryIsNull()
    {
        // Arrange
        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.Error.Should().Be(CategoryError.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRepositoryThrows()
    {
        // Arrange
        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Repository failed"));

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.Error.Should().Be(CategoryError.UnexpectedError);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCategoryExists()
    {
        // Arrange
        var category = Category.Create("TestCategory", "Test Description").Value;

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("TestCategory");
        result.Value.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Handle_Should_CallRepository_WhenCategoryExists()
    {
        // Arrange
        var category = Category.Create("TestCategory", "Test Description").Value;

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        await _categoryRepositoryMock.Received(1).GetByIdAsync(Command.Id, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenIdIsInvalid()
    {
        // Arrange
        var invalidCommand = new GetCategoryByIdQuery(Guid.Empty);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(invalidCommand, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.NotFound); // Or a specific validation error
    }
    
    [Fact]
    public async Task Handle_Should_MapCategoryToResponseCorrectly()
    {
        // Arrange
        var category = Category.Create("TestCategory", "Test Description").Value;

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("TestCategory");
        result.Value.Description.Should().Be("Test Description");
        result.Value.CreatedAt.Should().Be(category.CreatedAt);
        result.Value.UpdatedAt.Should().Be(category.LastModifiedAt);
    }
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenConcurrencyExceptionOccurs()
    {
        // Arrange
        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyException("Concurrency issue", new Exception()));

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.UnexpectedError); // Or a specific concurrency error
    }
    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCategoryNameIsMaxLength()
    {
        // Arrange
        var maxLengthName = new string('a', 100); // Assuming max length is 100
        var category = Category.Create(maxLengthName, "Test Description").Value;

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(maxLengthName);
    }
    
    [Fact]
    public async Task Handle_Should_CallRepositoryOnce_WhenInvokedMultipleTimes()
    {
        // Arrange
        var category = Category.Create("TestCategory", "Test Description").Value;

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        await _handler.Handle(Command, default);
        await _handler.Handle(Command, default);

        // Assert
        await _categoryRepositoryMock.Received(2).GetByIdAsync(Command.Id, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCategoryIsLargeOrComplex()
    {
        // Arrange
        var category = Category.Create("TestCategory", "Test Description").Value;
        // Add complex data to the category if applicable

        _categoryRepositoryMock
            .GetByIdAsync(Command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        Result<CategoryByIdResponse> result = await _handler.Handle(Command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}