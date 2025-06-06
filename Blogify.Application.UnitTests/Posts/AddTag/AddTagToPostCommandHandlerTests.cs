using Blogify.Application.Posts.AddTagToPost;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

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
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new AddTagToPostCommand(Guid.NewGuid(), Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TagNotFound_ReturnsTagNotFoundFailure()
    {
        // Arrange
        var post = CreateDraftPost();
        var command = new AddTagToPostCommand(post.Id, Guid.NewGuid());
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepository.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns((Tag)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TagAlreadyExists_ReturnsSuccessWithoutUpdate()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateDraftPost();
        post.AddTag(tag); // Tag already exists
        var command = new AddTagToPostCommand(post.Id, tag.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepository.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Tags.ShouldHaveSingleItem(); // No duplicates added
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidTag_AddsTagAndUpdatesPost()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateDraftPost();
        var command = new AddTagToPostCommand(post.Id, tag.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepository.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Tags.ShouldContain(t => t.Id == tag.Id);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    #region Helper Methods

    private static Post CreateDraftPost()
    {
        var title = PostTitle.Create("Test Post").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Test Excerpt").Value;
        return Post.Create(title, content, excerpt, Guid.NewGuid()).Value;
    }

    private Tag CreateTag()
    {
        return Tag.Create(TagName.Create("TestTag").Value.Value).Value;
    }

    #endregion
}
