using Blogify.Application.Categories.UpdateCategory;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.Update;

public class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateCategoryAndReturnSuccess()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();

        // Create a category with the same Id as the one being queried
        var category = Category.Create("OldName", "OldDescription").Value;
        category.GetType().GetProperty("Id").SetValue(category, categoryId); // Explicitly set the Id

        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(categoryId, "NewName", "NewDescription"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify that UpdateAsync was called with the correct updated category
        await categoryRepository.Received(1).UpdateAsync(
            Arg.Is<Category>(c =>
                c.Id == categoryId &&
                c.Name.Value == "NewName" &&
                c.Description.Value == "NewDescription"),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns((Category)null);

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(categoryId, "NewName", "NewDescription"),
            CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        var category = Category.Create("OldName", "OldDescription").Value;
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(categoryId, "", "NewDescription"),
            CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Description.ShouldBe("Name cannot be empty.");
        await categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        var category = Category.Create("OldName", "OldDescription").Value;
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);
        categoryRepository.UpdateAsync(category, CancellationToken.None).Throws(new Exception("Repository failed"));

        // Act & Assert
        await Should.ThrowAsync<Exception>(() =>
            handler.Handle(new UpdateCategoryCommand(categoryId, "NewName", "NewDescription"), CancellationToken.None));
        await categoryRepository.Received(1).UpdateAsync(Arg.Any<Category>(), CancellationToken.None);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNoChangesAreMade()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        var category = Category.Create("TestCategory", "Test Description").Value;
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(categoryId, "TestCategory", "Test Description"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>(), CancellationToken.None);
    }
    
    [Fact]
    public async Task Handle_ShouldCallUpdateAsync_WhenNameIsUpdated()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        var category = Category.Create("OldName", "Test Description").Value;
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(
            new UpdateCategoryCommand(categoryId, "NewName", "Test Description"), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await categoryRepository.Received(1).UpdateAsync(Arg.Any<Category>(), CancellationToken.None);
    }
    
    [Fact]
    public async Task Handle_ShouldCallUpdateAsync_WhenDescriptionIsUpdated()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new UpdateCategoryCommandHandler(categoryRepository);
        var categoryId = Guid.NewGuid();
        var category = Category.Create("TestCategory", "OldDescription").Value;
        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None).Returns(category);

        // Act
        var result = await handler.Handle(
            new UpdateCategoryCommand(categoryId, "TestCategory", "NewDescription"), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await categoryRepository.Received(1).UpdateAsync(Arg.Any<Category>(), CancellationToken.None);
    }
}