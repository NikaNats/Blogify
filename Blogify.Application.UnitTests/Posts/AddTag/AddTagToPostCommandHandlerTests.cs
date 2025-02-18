using Blogify.Application.Posts.AddTagToPost;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts.AddTag;

public class AddTagToPostCommandHandlerTests
{
    private readonly AddTagToPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public AddTagToPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new AddTagToPostCommandHandler(_postRepository, _tagRepository);
    }

    [Fact]
    public async Task Handle_PostExists_TagExists_TagAddedSuccessfully()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var post = CreateValidPost();
        var tag = CreateValidTag();

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns(post);
        _tagRepository.GetByIdAsync(tagId, CancellationToken.None).Returns(tag);

        // Act
        var result = await _handler.Handle(new AddTagToPostCommand(postId, tagId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess, "Handler should return success when adding a tag to a post.");
        await _postRepository.Received(1).UpdateAsync(post, CancellationToken.None);
        Assert.Contains(tag, post.Tags);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns((Post)null);

        // Act
        var result = await _handler.Handle(new AddTagToPostCommand(postId, tagId), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess, "Handler should return failure when the post is not found.");
        Assert.Equal("Post.NotFound", result.Error.Code);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), CancellationToken.None);
        await _tagRepository.DidNotReceive().GetByIdAsync(tagId, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_TagNotFound_ReturnsFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var post = CreateValidPost();

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns(post);
        _tagRepository.GetByIdAsync(tagId, CancellationToken.None).Returns((Tag)null);

        // Act
        var result = await _handler.Handle(new AddTagToPostCommand(postId, tagId), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess, "Handler should return failure when the tag is not found.");
        Assert.Equal("Tag.NotFound", result.Error.Code);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_TagAlreadyAdded_ReturnsSuccessWithoutUpdating()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var post = CreateValidPost();
        var tag = CreateValidTag();

        post.AddTag(tag); // First addition

        _postRepository.GetByIdAsync(postId, CancellationToken.None).Returns(post);
        _tagRepository.GetByIdAsync(tagId, CancellationToken.None).Returns(tag);

        // Act
        var result = await _handler.Handle(new AddTagToPostCommand(postId, tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    // Helper Methods
    private static Post CreateValidPost()
    {
        var titleResult = PostTitle.Create("Valid Title");
        Assert.True(titleResult.IsSuccess, "PostTitle creation failed.");

        var contentResult =
            PostContent.Create(
                "This is a valid post content that meets the minimum length requirement of 100 characters. It must be long enough to pass the validation rules.");
        Assert.True(contentResult.IsSuccess, "PostContent creation failed.");

        var excerptResult = PostExcerpt.Create("Valid excerpt.");
        Assert.True(excerptResult.IsSuccess, "PostExcerpt creation failed.");

        var postResult = Post.Create(
            titleResult.Value,
            contentResult.Value,
            excerptResult.Value,
            Guid.NewGuid());

        Assert.True(postResult.IsSuccess, "Post creation failed.");
        return postResult.Value;
    }

    private static Tag CreateValidTag()
    {
        var tagResult = Tag.Create("ValidTagName");
        Assert.True(tagResult.IsSuccess, "Tag creation failed.");
        return tagResult.Value;
    }
}