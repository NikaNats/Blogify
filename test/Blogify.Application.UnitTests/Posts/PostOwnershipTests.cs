using Blogify.Application.Posts.DeletePost;
using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts;

public class PostOwnershipTests
{
    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private Post ArrangePost(Guid authorId)
    {
        var post = Post.Create("Title", new string('a', 100), "Excerpt", authorId).Value;
        _posts.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        return post;
    }

    [Fact]
    public async Task Update_ByNonOwner_ReturnsForbidden()
    {
        var post = ArrangePost(Guid.NewGuid());
        var handler = new UpdatePostCommandHandler(_posts, _unitOfWork, TestUserContexts.User(Guid.NewGuid()));

        var result = await handler.Handle(
            new UpdatePostCommand(post.Id, "New title", new string('b', 100), "New excerpt"),
            CancellationToken.None);

        result.Error.ShouldBe(PostErrors.Forbidden);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ByNonOwner_ReturnsForbidden()
    {
        var post = ArrangePost(Guid.NewGuid());
        var handler = new DeletePostCommandHandler(_posts, _unitOfWork, TestUserContexts.User(Guid.NewGuid()));

        var result = await handler.Handle(new DeletePostCommand(post.Id), CancellationToken.None);

        result.Error.ShouldBe(PostErrors.Forbidden);
        await _posts.DidNotReceive().DeleteAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ByOwner_Succeeds()
    {
        var authorId = Guid.NewGuid();
        var post = ArrangePost(authorId);
        var handler = new DeletePostCommandHandler(_posts, _unitOfWork, TestUserContexts.User(authorId));

        var result = await handler.Handle(new DeletePostCommand(post.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Delete_Anonymous_ReturnsForbidden()
    {
        var post = ArrangePost(Guid.NewGuid());
        var handler = new DeletePostCommandHandler(
            _posts, _unitOfWork, Substitute.For<Blogify.Application.Abstractions.Authentication.IUserContext>());

        var result = await handler.Handle(new DeletePostCommand(post.Id), CancellationToken.None);

        result.Error.ShouldBe(PostErrors.Forbidden);
    }
}
