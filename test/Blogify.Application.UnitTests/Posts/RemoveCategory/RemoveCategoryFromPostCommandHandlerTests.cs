using Blogify.Application.Posts.RemoveCategoryFromPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.RemoveCategory;

public class RemoveCategoryFromPostCommandHandlerTests
{
    // --- CHANGED: Added IUnitOfWork mock ---
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly RemoveCategoryFromPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public RemoveCategoryFromPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>(); // Mock it

        // --- CHANGED: Inject all required dependencies ---
        _handler = new RemoveCategoryFromPostCommandHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveCategoryFromPostCommand(Guid.NewGuid(), Guid.NewGuid());
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldReturnFailure()
    {
        // Arrange
        var post = TestFactory.CreatePost();
        var command = new RemoveCategoryFromPostCommand(post.Id, Guid.NewGuid());
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepositoryMock.GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryIsAssigned_ShouldRemoveCategoryAndSaveChanges()
    {
        // Arrange
        var category = TestFactory.CreateCategory();
        var post = TestFactory.CreatePost();

        // --- FIXED: Use the correct method for setup ---
        post.AssignToCategory(category);
        post.ClearDomainEvents(); // Isolate the 'remove' action

        var command = new RemoveCategoryFromPostCommand(post.Id, category.Id);
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepositoryMock.GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // --- FIXED: Assert on the correct ID-based collection ---
        post.CategoryIds.ShouldNotContain(category.Id);

        // --- FIXED: Assert that the transaction was committed ---
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost()
        {
            var result = Post.Create(
                "Test Post",
                new string('a', 101),
                "Test Excerpt",
                Guid.NewGuid());
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        internal static Category CreateCategory()
        {
            var nameResult = CategoryName.Create("TestCategory");
            var descriptionResult = CategoryDescription.Create("Description");

            // Ensure both results are successful before proceeding
            nameResult.IsSuccess.ShouldBeTrue();
            descriptionResult.IsSuccess.ShouldBeTrue();

            var result = Category.Create(
                nameResult.Value.Value, // Extract the string value from CategoryName
                descriptionResult.Value.Value // Extract the string value from CategoryDescription
            );

            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}