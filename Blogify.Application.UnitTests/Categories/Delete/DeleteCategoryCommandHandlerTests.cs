using Blogify.Application.Categories.DeleteCategory;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using NSubstitute;
using Shouldly;

// Often useful, though not strictly needed for this fix

namespace Blogify.Application.UnitTests.Categories.Delete;

public class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly DeleteCategoryCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public DeleteCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock, _unitOfWorkMock);
    }

    private static Category CreateTestCategory()
    {
        var categoryResult = Category.Create("Test Category", "A category to be deleted.");
        categoryResult.IsSuccess.ShouldBeTrue("Test setup failed: could not create category.");
        return categoryResult.Value;
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldDeleteCategoryAndSaveChanges()
    {
        // Arrange
        var category = CreateTestCategory();
        var command = new DeleteCategoryCommand(category.Id);

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _categoryRepositoryMock.Received(1).DeleteAsync(category, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NotFound);
        await _categoryRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        var category = CreateTestCategory();
        var command = new DeleteCategoryCommand(category.Id);
        var expectedException = new Exception("Database commit failed");

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // FIX APPLIED HERE:
        // We now use Task.FromException<int>() to create a faulted Task<int>, which
        // matches the return type of SaveChangesAsync.
        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(expectedException));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.ShouldBe(expectedException);
    }
}