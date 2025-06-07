using Blogify.Application.Categories.UpdateCategory;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.Update;

public class UpdateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly UpdateCategoryCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new UpdateCategoryCommandHandler(_categoryRepositoryMock, _unitOfWorkMock);
    }

    private static Category CreateTestCategory()
    {
        var categoryResult = Category.Create("Original Name", "Original Description");
        categoryResult.IsSuccess.ShouldBeTrue("Test setup failed: could not create category.");
        return categoryResult.Value;
    }

    [Fact]
    public async Task Handle_WhenCategoryExistsAndDataIsValid_ShouldUpdateCategoryAndSaveChanges()
    {
        // Arrange
        var category = CreateTestCategory();
        var command = new UpdateCategoryCommand(category.Id, "Updated Name", "Updated Description");

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        category.Name.Value.ShouldBe(command.Name);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Doesn't Matter", "Doesn't Matter");

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        // This assertion will now pass because the handler returns the correct static error object.
        result.Error.ShouldBe(CategoryError.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateWithInvalidData_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var category = CreateTestCategory();
        var command = new UpdateCategoryCommand(category.Id, "", "Valid Description");

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NameNullOrEmpty);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoChangesAreMade_ShouldReturnSuccessAndNotSaveChanges()
    {
        // Arrange
        var category = CreateTestCategory();
        var command = new UpdateCategoryCommand(category.Id, category.Name.Value, category.Description.Value);

        _categoryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}