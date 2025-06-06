using Blogify.Application.Posts.RemoveTagFromPost;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.RemoveTag;

public class RemoveTagFromPostCommandHandlerTests
{
    private readonly RemoveTagFromPostCommandHandler _handler;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public RemoveTagFromPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new RemoveTagFromPostCommandHandler(_postRepository, _tagRepository);
    }

    [Fact]
    public async Task Handle_PostNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new RemoveTagFromPostCommand(Guid.NewGuid(), Guid.NewGuid());
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
        var command = new RemoveTagFromPostCommand(post.Id, Guid.NewGuid());
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
    public async Task Handle_ValidTag_RemovesTagAndUpdatesPost()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateDraftPost();
        post.AddTag(tag);
        var command = new RemoveTagFromPostCommand(post.Id, tag.Id);
        _postRepository.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepository.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Tags.ShouldNotContain(t => t.Id == tag.Id);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    private Post CreateDraftPost()
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
}
