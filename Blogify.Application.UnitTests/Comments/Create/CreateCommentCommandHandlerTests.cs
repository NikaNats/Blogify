using Blogify.Application.Comments.CreateComment;
using Blogify.Domain.Comments;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Comments.Create;

public class CreateCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly CreateCommentCommandHandler _handler;

    public CreateCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _handler = new CreateCommentCommandHandler(_commentRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithCommentId()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.NewGuid());
        _commentRepository.AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _commentRepository.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidContent_ReturnsFailure()
    {
        // Arrange
        var command = new CreateCommentCommand("", Guid.NewGuid(), Guid.NewGuid()); // Empty content is invalid

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.NewGuid());
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(command, cancellationToken));
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }
}