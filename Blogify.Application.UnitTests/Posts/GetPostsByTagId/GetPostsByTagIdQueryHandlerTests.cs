using Blogify.Application.Posts.GetPostsByCategoryId;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Posts.GetPostsByTagId;

public class GetPostsByCategoryIdQueryHandlerTests
{
    private readonly GetPostsByCategoryIdQueryHandler _handler;
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();

    public GetPostsByCategoryIdQueryHandlerTests()
    {
        _handler = new GetPostsByCategoryIdQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_PostsExistForCategory_ReturnsProperlyMappedResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var category = Category.Create("Technology", "Tech content").Value;
        var tag = Tag.Create("ASP.NET").Value;
        var post = CreatePublishedPost(authorId);

        // Establish proper bidirectional relationships
        category.AddPost(post);
        post.AddCategory(category); // Add this line

        tag.AddPost(post);
        post.AddTag(tag); // Add this line

        post.AddComment("Great post!", authorId);

        _postRepository.GetByCategoryIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(new List<Post> { post });

        // Act
        var result = await _handler.Handle(new GetPostsByCategoryIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var response = result.Value[0];
        response.Categories.Should().ContainSingle(c => c.Id == category.Id);
        response.Tags.Should().ContainSingle(t => t.Id == tag.Id);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _postRepository.GetByCategoryIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        // Act
        var result = await _handler.Handle(new GetPostsByCategoryIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepositoryFailure_ReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var exception = new Exception("Database failure");

        _postRepository.GetByCategoryIdAsync(categoryId, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(new GetPostsByCategoryIdQuery(categoryId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Posts.RetrievalFailed");
        result.Error.Description.Should().Contain(categoryId.ToString());
        // result.Error.Code.Should().Be(exception);
    }

    [Fact]
    public async Task Handle_ValidRequest_InvokesRepositoryWithCorrectParameters()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await _handler.Handle(new GetPostsByCategoryIdQuery(categoryId), cancellationToken);

        // Assert
        await _postRepository.Received(1)
            .GetByCategoryIdAsync(categoryId, cancellationToken);
    }

    private Post CreatePublishedPost(Guid authorId)
    {
        var title = PostTitle.Create("Test Title").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;

        var postResult = Post.Create(title, content, excerpt, authorId);
        postResult.IsSuccess.Should().BeTrue("Test data setup failed");

        var post = postResult.Value;
        post.Publish();
        return post;
    }
}