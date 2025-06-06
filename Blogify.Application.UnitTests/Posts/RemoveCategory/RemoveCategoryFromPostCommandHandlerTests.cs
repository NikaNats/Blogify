using Blogify.Application.Posts.RemoveCategoryFromPost;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.RemoveCategory;

public class RemoveCategoryFromPostCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly RemoveCategoryFromPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public RemoveCategoryFromPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _handler = new RemoveCategoryFromPostCommandHandler(_postRepository, _categoryRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new RemoveCategoryFromPostCommand(Guid.NewGuid(), Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ReturnsCategoryNotFoundFailure()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new RemoveCategoryFromPostCommand(post.Id, Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepository.GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CategoryError.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCategory_RemovesCategoryAndUpdatesPost()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreateDraftPost();
        post.AddCategory(category);
        var command = new RemoveCategoryFromPostCommand(post.Id, category.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepository.GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Categories.ShouldNotContain(c => c.Id == category.Id);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    private Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }

    private Category CreateCategory()
    {
        return Category.Create(CategoryName.Create("TestCategory").Value.Value,
            CategoryDescription.Create("Description").Value.Value).Value;
    }
}
