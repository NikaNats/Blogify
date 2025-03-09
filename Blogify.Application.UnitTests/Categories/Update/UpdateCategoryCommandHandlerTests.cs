using System.Reflection;
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
        var originalName = "OldName";
        var originalDescription = "OldDescription";

        // Create category with known ID using production code
        var category = Category.Create(originalName, originalDescription).Value;

        // Get the private constructor
        var constructor = typeof(Category).GetConstructors(
            BindingFlags.NonPublic | BindingFlags.Instance
        )[0];

        // Reconstruct category with desired ID
        var categoryWithId = (Category)constructor.Invoke(new object[]
        {
            categoryId,
            category.Name,
            category.Description
        });

        categoryRepository.GetByIdAsync(categoryId, CancellationToken.None)
            .Returns(categoryWithId);

        // Act
        var result = await handler.Handle(
            new UpdateCategoryCommand(categoryId, "NewName", "NewDescription"),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await categoryRepository.Received(1).UpdateAsync(
            Arg.Is<Category>(c =>
                c.Id == categoryId &&
                c.Name.Value == "NewName" &&
                c.Description.Value == "NewDescription"),
            Arg.Any<CancellationToken>()
        );
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
        result.Error.Description.ShouldBe(CategoryError.NameNullOrEmpty.Description); // Updated
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
        var result = await handler.Handle(new UpdateCategoryCommand(categoryId, "TestCategory", "Test Description"),
            CancellationToken.None);

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