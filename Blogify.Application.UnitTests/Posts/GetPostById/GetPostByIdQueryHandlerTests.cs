using Blogify.Application.Posts.GetPostById;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.GetPostById;

public class GetPostByIdQueryHandlerTests
{
    private readonly GetPostByIdQueryHandler _handler;
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new GetPostByIdQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var query = new GetPostByIdQuery(Guid.NewGuid());
        _postRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ValidPost_ReturnsMappedResponse()
    {
        // Arrange
        var post = CreateDraftPost();
        var query = new GetPostByIdQuery(post.Id);
        _postRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(post);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(post.Id);
        result.Value.Title.Should().Be(post.Title.Value);
    }

    private Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }
}