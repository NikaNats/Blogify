using Blogify.Application.Exceptions;
using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Posts.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly CreatePostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new CreatePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithPostId()
    {
        // Arrange
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create("This is a valid content that meets the minimum length requirement.").Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid(),
            Guid.NewGuid());

        var postResult = Post.Create(
            command.Title,
            command.Content,
            command.Excerpt,
            command.AuthorId);

        // Ensure the post creation was successful
        postResult.IsSuccess.Should().BeTrue("Post creation should succeed with valid input.");
        var post = postResult.Value;

        _postRepository.AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue("Command handling should succeed with valid input.");
        result.Value.Should().Be(post.Id);
        await _postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Id == post.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrencyException_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create("This is a valid content that meets the minimum length requirement.").Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid(),
            Guid.NewGuid());

        _postRepository
            .AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Throws(new ConcurrencyException("Concurrency conflict occurred.", new Exception()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.Overlap);

        // Ensure accessing Value property throws InvalidOperationException
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failure result can't be accessed.");
    }
}