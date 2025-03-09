using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Posts;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.UpdatePost;

/// <summary>
///     Unit tests for the UpdatePostCommandHandler class, ensuring it correctly handles
///     post updates, including retrieval, modification, persistence, and error scenarios.
/// </summary>
public class UpdatePostCommandHandlerTests
{
    private readonly UpdatePostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public UpdatePostCommandHandlerTests()
    {
        // Initialize mock repository and handler once per test class for consistency.
        // This keeps tests fast and avoids redundant setup while ensuring independence
        // through fresh mock behavior per test.
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new UpdatePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        var command = CreateValidCommand(Guid.NewGuid());
        _postRepository
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Post?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_PostExists_UpdatesPostAndPersistsChanges()
    {
        // Arrange
        var post = CreateDraftPost(Guid.NewGuid());
        var originalTitle = post.Title;
        var originalContent = post.Content;
        var originalExcerpt = post.Excerpt;

        _postRepository
            .GetByIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Post?>(post));

        var command = CreateValidCommand(post.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Title, post.Title);
        Assert.Equal(command.Content, post.Content);
        Assert.Equal(command.Excerpt, post.Excerpt);
        Assert.NotEqual(originalTitle, post.Title); // Ensure properties changed
        Assert.NotEqual(originalContent, post.Content);
        Assert.NotEqual(originalExcerpt, post.Excerpt);

        await _postRepository
            .Received(1)
            .UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PostIsArchived_DoesNotUpdatePropertiesButPersistsUnchangedPost()
    {
        // Arrange
        var post = CreateArchivedPost(Guid.NewGuid());
        var originalTitle = post.Title;
        var originalContent = post.Content;
        var originalExcerpt = post.Excerpt;

        _postRepository
            .GetByIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Post?>(post));

        var command = CreateValidCommand(post.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Current behavior: returns success regardless
        Assert.Equal(originalTitle, post.Title); // Properties unchanged due to Post.Update logic
        Assert.Equal(originalContent, post.Content);
        Assert.Equal(originalExcerpt, post.Excerpt);

        await _postRepository
            .Received(1)
            .UpdateAsync(post, Arg.Any<CancellationToken>());

        // Note: This test reflects the current implementation where post.Update's
        // failure (due to CanBeModified returning false) is ignored. Ideally,
        // the handler should check post.Update's Result and return it if it fails.
    }

    #region Helper Methods

    /// <summary>
    ///     Creates a valid UpdatePostCommand with the specified ID and default valid values.
    /// </summary>
    private static UpdatePostCommand CreateValidCommand(Guid postId)
    {
        var updatedTitle = PostTitle.Create("Updated Title");
        Assert.True(updatedTitle.IsSuccess, "Failed to create updated title.");

        var updatedContent =
            PostContent.Create(
                "This is the updated content. Lorem ipsum dolor sit amet, consectetur adipiscing elit. This ensures the string is long enough.");
        Assert.True(updatedContent.IsSuccess, "Failed to create updated content.");

        var updatedExcerpt = PostExcerpt.Create("Updated excerpt.");
        Assert.True(updatedExcerpt.IsSuccess, "Failed to create updated excerpt.");

        return new UpdatePostCommand(
            postId,
            updatedTitle.Value,
            updatedContent.Value,
            updatedExcerpt.Value
        );
    }

    /// <summary>
    ///     Creates a draft Post instance for testing, simulating a modifiable state.
    /// </summary>
    private static Post CreateDraftPost(Guid id)
    {
        var title = PostTitle.Create("Initial Title").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Initial excerpt.").Value;
        var authorId = Guid.NewGuid();

        var postResult = Post.Create(title, content, excerpt, authorId);
        Assert.True(postResult.IsSuccess, "Post creation failed unexpectedly.");

        var post = postResult.Value;
        // Use reflection or assume ID can be set for testing; here, we rely on Create assigning a new ID
        // and adjust the test to use that ID, but for control, we could enhance Post for testability.
        return post;
    }

    /// <summary>
    ///     Creates an archived Post instance for testing, simulating a non-modifiable state.
    /// </summary>
    private static Post CreateArchivedPost(Guid id)
    {
        var post = CreateDraftPost(id);
        var archiveResult = post.Archive();
        Assert.True(archiveResult.IsSuccess, "Post archiving failed unexpectedly.");
        return post;
    }

    #endregion
}