using Blogify.Application.Exceptions;
using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Posts;
using Shouldly;
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
        var command = CreateValidPostCommand();
        _postRepository.AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrencyException_ReturnsFailure()
    {
        // Arrange
        var command = CreateValidPostCommand();
        _postRepository
            .AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Throws(new ConcurrencyException("The current Post is overlapping with an existing one.", new Exception()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(PostErrors.Overlap.Code); // Explicitly check error code
        result.Error.Description.ShouldBe("The current Post is overlapping with an existing one."); // Updated to match the thrown message
    }


    #region Helper Methods

    private static CreatePostCommand CreateValidPostCommand()
    {
        return new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value, // Valid content: exactly 100 characters
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());
    }

    #endregion
}
