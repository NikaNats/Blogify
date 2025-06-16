using Blogify.Application.Posts.AssignCategoriesToPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.AssignCategories;

public class AssignCategoriesToPostCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly AssignCategoriesToPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public AssignCategoriesToPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new AssignCategoriesToPostCommandHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var command = new AssignCategoriesToPostCommand(Guid.NewGuid(), [Guid.NewGuid()]);
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenACategoryIsNotFound_ShouldReturnCategoryNotFoundFailure()
    {
        // Arrange
        var post = TestFactory.CreatePost();
        var validCategory = TestFactory.CreateCategory();
        var invalidCategoryId = Guid.NewGuid();
        var command = new AssignCategoriesToPostCommand(post.Id, [validCategory.Id, invalidCategoryId]);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);

        // --- FIXED: Mock the GetByIdAsync calls precisely ---
        _categoryRepositoryMock.GetByIdAsync(validCategory.Id, Arg.Any<CancellationToken>()).Returns(validCategory);
        _categoryRepositoryMock.GetByIdAsync(invalidCategoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null); // This category is not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCategories_ShouldAssignCategoryIdsAndSaveChanges()
    {
        // Arrange
        var category1 = TestFactory.CreateCategory();
        var category2 = TestFactory.CreateCategory();
        var post = TestFactory.CreatePost();
        var command = new AssignCategoriesToPostCommand(post.Id, [category1.Id, category2.Id]);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepositoryMock.GetByIdAsync(category1.Id, Arg.Any<CancellationToken>()).Returns(category1);
        _categoryRepositoryMock.GetByIdAsync(category2.Id, Arg.Any<CancellationToken>()).Returns(category2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.CategoryIds.ShouldContain(category1.Id);
        post.CategoryIds.ShouldContain(category2.Id);
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
            var nameResult = CategoryName.Create("TestCategory" + Guid.NewGuid());
            nameResult.IsSuccess.ShouldBeTrue();
            var descriptionResult = CategoryDescription.Create("Description");
            descriptionResult.IsSuccess.ShouldBeTrue();

            var result = Category.Create(nameResult.Value.Value, descriptionResult.Value.Value);
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}