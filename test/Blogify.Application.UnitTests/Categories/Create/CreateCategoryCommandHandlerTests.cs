using Blogify.Application.Categories.CreateCategory;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.Create;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly CreateCategoryCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenNameIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateCategoryCommand("New Category", "A valid description.");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        await _categoryRepositoryMock.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCategoryCommand("", "Description"); // Empty name is invalid

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NameNullOrEmpty);
        await _categoryRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldReturnConflictFailure()
    {
        // Arrange
        var command = new CreateCategoryCommand("Existing Category", "Description");
        _categoryRepositoryMock.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(TestFactory.CreateCategory(command.Name, command.Description));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NameAlreadyExists);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateCategoryCommand("Good Category", "Good Description");
        var expectedException = new Exception("Database error");

        // Mock the repository to throw when AddAsync is called
        _categoryRepositoryMock.AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        // Verify that the handler does NOT catch the exception and lets it bubble up.
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, default));
        exception.ShouldBe(expectedException);
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Category CreateCategory(string name, string description)
        {
            var result = Category.Create(name, description);
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}