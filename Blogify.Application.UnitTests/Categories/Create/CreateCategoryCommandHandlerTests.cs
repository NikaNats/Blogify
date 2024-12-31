using Blogify.Application.Categories.CreateCategory;
using Blogify.Domain.Categories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Categories.Create;

public class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCategoryAndReturnCategoryId()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new CreateCategoryCommandHandler(categoryRepository);
        var command = new CreateCategoryCommand("TestCategory", "Test Description");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        await categoryRepository.Received(1).AddAsync(Arg.Any<Category>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryCreationFails()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new CreateCategoryCommandHandler(categoryRepository);
        var command = new CreateCategoryCommand("", "Test Description"); // Invalid name

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        await categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var handler = new CreateCategoryCommandHandler(categoryRepository);
        var command = new CreateCategoryCommand("TestCategory", "Test Description");
        categoryRepository.AddAsync(Arg.Any<Category>(), CancellationToken.None)
            .Throws(new Exception("Repository failed"));

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        await categoryRepository.Received(1).AddAsync(Arg.Any<Category>(), CancellationToken.None);
    }
}