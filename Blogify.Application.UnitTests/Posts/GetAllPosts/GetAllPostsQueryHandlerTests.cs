using Blogify.Application.Posts.GetAllPosts;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetAllPosts;

public class GetAllPostsQueryHandlerTests
{
    private readonly GetAllPostsQueryHandler _handler;
    private readonly IPostRepository _postRepository;

    public GetAllPostsQueryHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new GetAllPostsQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_NoPosts_ReturnsEmptyList()
    {
        // Arrange
        _postRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Post>());

        // Act
        var result = await _handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_MultiplePosts_ReturnsMappedResponses()
    {
        // Arrange
        var posts = new List<Post>
        {
            CreateDraftPost(),
            CreatePublishedPost()
        };
        _postRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(posts);

        // Act
        var result = await _handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Id.ShouldBe(posts[0].Id);
        result.Value[1].Id.ShouldBe(posts[1].Id);
        result.Value[1].Status.ShouldBe(PublicationStatus.Published);
    }

    #region Helper Methods

    private static Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }

    private Post CreatePublishedPost()
    {
        var post = CreateDraftPost();
        post.Publish();
        return post;
    }

    #endregion
}