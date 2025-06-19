using Blogify.Application.Abstractions.Authentication;
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
    private readonly IUserContext _userContextMock;

    public CreatePostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>();

        _handler = new CreatePostCommandHandler(
            _postRepositoryMock,
            _unitOfWorkMock,
            _userContextMock);
    }

    private static CreatePostCommand CreateValidCommand()
    {
        return new CreatePostCommand(
            "Valid Title",
            new string('a', 100),
            "Valid Excerpt"
        );
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldAddPostAndSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var authenticatedUserId = Guid.NewGuid();

        _userContextMock.UserId.Returns(authenticatedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        await _postRepositoryMock.Received(1).AddAsync(
            Arg.Is<Post>(p => p.AuthorId == authenticatedUserId),
            Arg.Any<CancellationToken>());

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var commandWithInvalidData = new CreatePostCommand(
            "",
            new string('a', 100),
            "Valid Excerpt");

        // Act
        var result = await _handler.Handle(commandWithInvalidData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.TitleEmpty);

        await _postRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDbThrowsConcurrencyException_ShouldReturnOverlapError()
    {
        // Arrange
        var command = CreateValidCommand();
        var authenticatedUserId = Guid.NewGuid();
        var concurrencyException = new ConcurrencyException("Concurrency conflict.", new Exception());

        _userContextMock.UserId.Returns(authenticatedUserId);

        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(concurrencyException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.Overlap);
    }
}