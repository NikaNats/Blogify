using Blogify.Application.Posts.GetPostsByAuthorId;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

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
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
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
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].AuthorId.ShouldBe(authorId);
        result.Value[1].AuthorId.ShouldBe(authorId);
    }

    private Post CreatePostWithAuthor(Guid authorId)
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, authorId).Value;
    }
}