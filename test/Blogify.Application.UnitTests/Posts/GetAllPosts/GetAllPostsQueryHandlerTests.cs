using Blogify.Application.Posts.GetAllPosts;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetAllPosts;

public class GetAllPostsQueryHandlerTests
{
    // --- CHANGED: Added ITagRepository mock ---
    private readonly GetAllPostsQueryHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;

    public GetAllPostsQueryHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();

        // --- CHANGED: Inject all required dependencies ---
        _handler = new GetAllPostsQueryHandler(_postRepositoryMock, _tagRepositoryMock);
    }

    [Fact]
    public async Task Handle_WhenNoPostsExist_ShouldReturnEmptyList()
    {
        // Arrange
        _postRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Post>());

        // Act
        var result = await _handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPostsExist_ShouldReturnMappedResponses()
    {
        // Arrange
        var tag1 = TestFactory.CreateTag("Tech");
        var post1 = TestFactory.CreatePost();
        post1.AddTag(tag1);

        var post2 = TestFactory.CreatePost(true);

        var posts = new List<Post> { post1, post2 };

        // --- FIXED: Mock all repository calls made by the handler ---
        _postRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(posts);
        _tagRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Tag> { tag1 });

        // Act
        var result = await _handler.Handle(new GetAllPostsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);

        var responseForPost1 = result.Value.Single(r => r.Id == post1.Id);
        responseForPost1.Status.ShouldBe(PublicationStatus.Draft);
        responseForPost1.Tags.ShouldHaveSingleItem().Name.ShouldBe(tag1.Name.Value);

        var responseForPost2 = result.Value.Single(r => r.Id == post2.Id);
        responseForPost2.Status.ShouldBe(PublicationStatus.Published);
        responseForPost2.Tags.ShouldBeEmpty();
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost(bool isPublished = false)
        {
            var authorId = Guid.NewGuid();

            var result = Post.Create(
                "Test Post",
                new string('a', 101),
                "An excerpt.",
                authorId
            );

            result.IsSuccess.ShouldBeTrue();
            var post = result.Value;

            if (isPublished) post.Publish();

            return post;
        }

        internal static Tag CreateTag(string name)
        {
            var result = Tag.Create(name);
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}