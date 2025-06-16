using Blogify.Application.Tags.UpdateTag;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class UpdateTagCommandHandlerTests
{
    private readonly UpdateTagCommandHandler _handler;
    private readonly ITagRepository _tagRepository;

    public UpdateTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new UpdateTagCommandHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WhenTagExistsAndNameIsValid_Should_SucceedAndUpdateTag()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand(tagId, "Updated Name");
        var existingTag = Tag.Create("Original Name").Value;

        _tagRepository.GetByIdAsync(tagId, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify that UpdateAsync was called on the repository
        await _tagRepository.Received(1)
            .UpdateAsync(Arg.Is<Tag>(t => t.Name.Value == command.Name), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_Should_ReturnNotFoundError()
    {
        // Arrange
        var command = new UpdateTagCommand(Guid.NewGuid(), "Updated Name");

        _tagRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Tag)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNameIsInvalid_Should_ReturnDomainValidationError()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var command = new UpdateTagCommand(tagId, ""); // Invalid name
        var existingTag = Tag.Create("Original Name").Value;

        _tagRepository.GetByIdAsync(tagId, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NameEmpty);

        // Verify UpdateAsync was never called
        await _tagRepository.DidNotReceive().UpdateAsync(Arg.Any<Tag>(), Arg.Any<CancellationToken>());
    }
}