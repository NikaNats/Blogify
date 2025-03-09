using Blogify.Application.Posts.PublishPost;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.PublishPost;

public class PublishPostCommandHandlerTests
{
    private readonly PublishPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public PublishPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new PublishPostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new PublishPostCommand(Guid.NewGuid());
        _postRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidPost_PublishesPostAndUpdates()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new PublishPostCommand(post.Id);
        _postRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Status.Should().Be(PublicationStatus.Published);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    private Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }
}