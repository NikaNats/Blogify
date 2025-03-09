using Blogify.Application.Posts.AssignCategoriesToPost;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.AssignCategories;

public class AssignCategoriesToPostCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly AssignCategoriesToPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public AssignCategoriesToPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _handler = new AssignCategoriesToPostCommandHandler(_postRepository, _categoryRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new AssignCategoriesToPostCommand(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ReturnsCategoryNotFoundFailure()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new AssignCategoriesToPostCommand(post.Id, new List<Guid> { Guid.NewGuid() });
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepository.GetByIdAsync(command.CategoryIds[0], Arg.Any<CancellationToken>()).Returns((Category)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CategoryError.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCategories_AssignsCategoriesAndUpdatesPost()
    {
        // Arrange
        var category1 = CreateCategory();
        var category2 = CreateCategory();
        var post = CreateDraftPost();
        var command = new AssignCategoriesToPostCommand(post.Id, new List<Guid> { category1.Id, category2.Id });
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _categoryRepository.GetByIdAsync(category1.Id, Arg.Any<CancellationToken>()).Returns(category1);
        _categoryRepository.GetByIdAsync(category2.Id, Arg.Any<CancellationToken>()).Returns(category2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Categories.Should().Contain(c => c.Id == category1.Id);
        post.Categories.Should().Contain(c => c.Id == category2.Id);
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