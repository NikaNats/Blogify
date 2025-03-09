using Blogify.Application.Posts.GetPostsByAuthorId;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.GetPostsByAuthorId;

public class GetPostsByAuthorIdQueryHandlerTests
{
    private readonly GetPostsByAuthorIdQueryHandler _handler;
    private readonly IPostRepository _postRepository;

    public GetPostsByAuthorIdQueryHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new GetPostsByAuthorIdQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_NoPostsForAuthor_ReturnsEmptyList()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        _postRepository.GetByAuthorIdAsync(authorId, Arg.Any<CancellationToken>()).Returns(new List<Post>());

        // Act
        var result = await _handler.Handle(new GetPostsByAuthorIdQuery(authorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PostsForAuthor_ReturnsMappedResponses()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var posts = new List<Post> { CreatePostWithAuthor(authorId), CreatePostWithAuthor(authorId) };
        _postRepository.GetByAuthorIdAsync(authorId, Arg.Any<CancellationToken>()).Returns(posts);

        // Act
        var result = await _handler.Handle(new GetPostsByAuthorIdQuery(authorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AuthorId.Should().Be(authorId);
        result.Value[1].AuthorId.Should().Be(authorId);
    }

    private Post CreatePostWithAuthor(Guid authorId)
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, authorId).Value;
    }
}