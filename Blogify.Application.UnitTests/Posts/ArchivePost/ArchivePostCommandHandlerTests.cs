using Blogify.Application.Posts.ArchivePost;
using Blogify.Domain.Posts;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.ArchivePost;

public class ArchivePostCommandHandlerTests
{
    private readonly ArchivePostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public ArchivePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new ArchivePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostExists_PostArchivedSuccessfully()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = CreateValidPost();

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns(post);

        // Act
        var result = await _handler.Handle(new ArchivePostCommand(postId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess, "Handler should return success when archiving a post.");
        await _postRepository.Received(1).UpdateAsync(post, CancellationToken.None);
        Assert.Equal(PostStatus.Archived, post.Status);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns((Post)null);

        // Act
        var result = await _handler.Handle(new ArchivePostCommand(postId), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess, "Handler should return failure when the post is not found.");
        Assert.Equal("Post.NotFound", result.Error.Code);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_PostAlreadyArchived_ReturnsSuccessWithoutUpdating()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = CreateValidPost();
        post.Archive();

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns(post);

        // Act
        var result = await _handler.Handle(new ArchivePostCommand(postId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess, "Handler should return success when the post is already archived.");
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), CancellationToken.None);
    }

    // Helper Method
    private static Post CreateValidPost()
    {
        var titleResult = PostTitle.Create("Valid Title");
        Assert.True(titleResult.IsSuccess, "PostTitle creation failed.");

        var contentResult =
            PostContent.Create(
                "This is a valid post content that meets the minimum length requirement of 100 characters. It must be long enough to pass the validation rules.");
        Assert.True(contentResult.IsSuccess, "PostContent creation failed.");

        var excerptResult = PostExcerpt.Create("Valid excerpt.");
        Assert.True(excerptResult.IsSuccess, "PostExcerpt creation failed.");

        var postResult = Post.Create(
            titleResult.Value,
            contentResult.Value,
            excerptResult.Value,
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.True(postResult.IsSuccess, "Post creation failed.");
        return postResult.Value;
    }
}