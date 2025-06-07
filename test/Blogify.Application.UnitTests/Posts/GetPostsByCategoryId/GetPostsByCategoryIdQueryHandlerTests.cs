using Blogify.Application.Posts.GetPostsByTagId;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetPostsByCategoryId;

public class GetPostsByTagIdQueryHandlerTests
{
    private readonly GetPostsByTagIdQueryHandler _handler;
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();

    public GetPostsByTagIdQueryHandlerTests()
    {
        _handler = new GetPostsByTagIdQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostsExistForTag_ReturnsProperlyMappedResponse()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var post = CreateValidPublishedPost(authorId);
        var tag = CreateAndAssociateTagWithPost(post);

        _postRepository.GetByTagIdAsync(tag.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Post> { post });

        // Act
        var result = await _handler.Handle(new GetPostsByTagIdQuery(tag.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value.Single();

        // Verify tag relationship
        response.Tags.ShouldContain(t => t.Id == tag.Id);

        // Cleanup relationships
        post.RemoveTag(tag);
        tag.RemovePost(post);
    }

    private static Tag CreateAndAssociateTagWithPost(Post post)
    {
        var tagResult = Tag.Create("Integration-Test-Tag");
        tagResult.IsSuccess.ShouldBeTrue("Test data setup failed");

        var tag = tagResult.Value;

        // Establish bidirectional relationship
        var addPostResult = tag.AddPost(post);
        var addTagResult = post.AddTag(tag);

        addPostResult.IsSuccess.ShouldBeTrue("Failed to add post to tag");
        addTagResult.IsSuccess.ShouldBeTrue("Failed to add tag to post");

        return tag;
    }

    [Fact]
    public async Task Handle_NoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _postRepository.GetByTagIdAsync(tagId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        // Act
        var result = await _handler.Handle(new GetPostsByTagIdQuery(tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await _handler.Handle(new GetPostsByTagIdQuery(tagId), cancellationToken);

        // Assert
        await _postRepository.Received(1)
            .GetByTagIdAsync(tagId, cancellationToken);
    }

    [Fact]
    public async Task Handle_RepositoryFailure_PropagatesError()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var expectedError = new Exception("Database failure");
        _postRepository.GetByTagIdAsync(tagId, Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedError);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(new GetPostsByTagIdQuery(tagId), CancellationToken.None));
    }

    private static Post CreateValidPublishedPost(Guid authorId)
    {
        var title = PostTitle.Create("Test Title").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test excerpt").Value;

        var postResult = Post.Create(title, content, excerpt, authorId);
        postResult.IsSuccess.ShouldBeTrue("Invalid test data setup");

        var post = postResult.Value;
        post.Publish();
        return post;
    }
}