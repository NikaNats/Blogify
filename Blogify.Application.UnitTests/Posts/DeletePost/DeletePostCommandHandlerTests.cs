using Blogify.Application.Exceptions;
using Blogify.Application.Posts.DeletePost;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.DeletePost;

public class DeletePostCommandHandlerTests
{
    private readonly DeletePostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new DeletePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new DeletePostCommand(Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _postRepository.DidNotReceive().DeleteAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidPost_DeletesPostAndReturnsSuccess()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new DeletePostCommand(post.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _postRepository.DeleteAsync(post, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _postRepository.Received(1).DeleteAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrencyException_ReturnsOverlapFailure()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new DeletePostCommand(post.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _postRepository.When(x => x.DeleteAsync(post, Arg.Any<CancellationToken>())).Do(x =>
            throw new ConcurrencyException("Concurrency conflict", new Exception()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.Overlap);
    }

    private Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }
}
