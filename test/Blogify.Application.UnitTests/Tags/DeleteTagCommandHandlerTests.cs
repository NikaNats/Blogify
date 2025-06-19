using Blogify.Application.Tags.DeleteTag;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class DeleteTagCommandHandlerTests
{
    private readonly DeleteTagCommandHandler _handler;
    private readonly ITagRepository _tagRepository;

    public DeleteTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteTagCommandHandler(_tagRepository, unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTagExists_Should_SucceedAndDeleteTag()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId);

        // Create a tag instance that we can control.
        var existingTag = Tag.Create("Test Tag").Value;

        typeof(Entity).GetProperty(nameof(Entity.Id))!
            .SetValue(existingTag, tagId);

        _tagRepository.GetByIdAsync(tagId, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _tagRepository.Received(1).DeleteAsync(tagId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_Should_ReturnNotFoundError()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId);

        _tagRepository.GetByIdAsync(tagId, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);

        // Verify that DeleteAsync was never called
        await _tagRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}