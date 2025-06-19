using Blogify.Application.Tags.UpdateTag;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class UpdateTagCommandHandlerTests
{
    private readonly UpdateTagCommandHandler _handler;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateTagCommandHandler(_tagRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTagExistsAndNameIsValid_ShouldSucceedAndUpdateTag()
    {
        // Arrange
        var command = new UpdateTagCommand(Guid.NewGuid(), "Updated Name");
        var existingTag = Tag.Create("Original Name").Value;

        _tagRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns((Tag)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_ShouldReturnNotFoundError()
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
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsInvalid_ShouldReturnDomainValidationError()
    {
        // Arrange
        var command = new UpdateTagCommand(Guid.NewGuid(), ""); // Invalid name
        var existingTag = Tag.Create("Original Name").Value;

        _tagRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NameEmpty);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsDuplicate_ShouldReturnConflictError()
    {
        // Arrange
        var command = new UpdateTagCommand(Guid.NewGuid(), "Duplicate Name");

        var tagToUpdate = Tag.Create("Original Name").Value;
        _tagRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(tagToUpdate);

        var otherTagWithSameName = Tag.Create(command.Name).Value;
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(otherTagWithSameName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.DuplicateName);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}