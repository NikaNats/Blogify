using Blogify.Application.Exceptions;
using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly CreatePostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public CreatePostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new CreatePostCommandHandler(_postRepositoryMock, _unitOfWorkMock);
    }

    private static CreatePostCommand CreateValidCommand()
    {
        var title = PostTitle.Create("Valid Title").Value;
        var content = PostContent.Create(new string('a', 100)).Value;
        var excerpt = PostExcerpt.Create("Valid Excerpt").Value;
        return new CreatePostCommand(title, content, excerpt, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldAddPostAndSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        await _postRepositoryMock.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        // FIX: Construct the command with a value that will cause Post.Create to fail,
        // such as a null title. This correctly simulates invalid data being passed to the handler.
        var commandWithInvalidData = new CreatePostCommand(
            null, // This will cause Post.Create to return a failure Result.
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(commandWithInvalidData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.TitleNull);

        await _postRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDbThrowsConcurrencyException_ShouldReturnOverlapError()
    {
        // Arrange
        var command = CreateValidCommand();
        var concurrencyException = new ConcurrencyException("Concurrency conflict.", new Exception());

        // We still configure the mock to throw the exception...
        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(concurrencyException);

        // Act
        // ...but now we expect the handler to CATCH it and return a failed Result.
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.Overlap);
    }
}