using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandHandlerTests
{
    private readonly AddCommentToPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public AddCommentToPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        var commentRepository = Substitute.For<ICommentRepository>();
        _handler = new AddCommentToPostCommandHandler(_postRepository, commentRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Test comment", Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Post.NotFound");
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PostNotPublished_ReturnsValidationFailure()
    {
        // Arrange
        var postTitleResult = PostTitle.Create("Test Post");
        var postContentResult = PostContent.Create(new string('a', 100)); // Valid input: 100 characters
        var postExcerptResult = PostExcerpt.Create("Test Excerpt");

        // Ensure all creations were successful
        postTitleResult.IsSuccess.ShouldBeTrue();
        postContentResult.IsSuccess.ShouldBeTrue();
        postExcerptResult.IsSuccess.ShouldBeTrue();

        var postResult = Post.Create(
            postTitleResult.Value,
            postContentResult.Value,
            postExcerptResult.Value,
            Guid.NewGuid()
        );

        // Ensure the post creation was successful
        postResult.IsSuccess.ShouldBeTrue();
        var post = postResult.Value;

        var command = new AddCommentToPostCommand(post.Id, "Test comment", Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(PostErrors.CommentToUnpublishedPost.Code);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PostPublished_AddsCommentAndReturnsSuccess()
    {
        // Arrange
        var postTitleResult = PostTitle.Create("Test Post");
        var postContentResult = PostContent.Create(new string('a', 100)); // Valid input: 100 characters
        var postExcerptResult = PostExcerpt.Create("Test Excerpt");

        // Ensure all creations were successful
        postTitleResult.IsSuccess.ShouldBeTrue();
        postContentResult.IsSuccess.ShouldBeTrue();
        postExcerptResult.IsSuccess.ShouldBeTrue();

        var postResult = Post.Create(
            postTitleResult.Value,
            postContentResult.Value,
            postExcerptResult.Value,
            Guid.NewGuid()
        );

        var post = postResult.Value;
        post.Publish();

        var command = new AddCommentToPostCommand(post.Id, "Test comment", Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Comments.ShouldContain(c => c.Content.Value == command.Content && c.AuthorId == command.AuthorId);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CommentCreationFails_ReturnsFailure()
    {
        // Arrange
        var postTitleResult = PostTitle.Create("Test Post");
        var postContentResult = PostContent.Create(new string('a', 100));
        var postExcerptResult = PostExcerpt.Create("Test Excerpt");

        // Ensure all creations were successful
        postTitleResult.IsSuccess.ShouldBeTrue();
        postContentResult.IsSuccess.ShouldBeTrue();
        postExcerptResult.IsSuccess.ShouldBeTrue();

        var postResult = Post.Create(
            postTitleResult.Value,
            postContentResult.Value,
            postExcerptResult.Value,
            Guid.NewGuid()
        );

        // Ensure the post creation was successful
        postResult.IsSuccess.ShouldBeTrue();
        var post = postResult.Value;

        post.Publish(); // Ensure the post is published
        var command = new AddCommentToPostCommand(post.Id, string.Empty, Guid.NewGuid()); // Invalid comment content
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }
}
