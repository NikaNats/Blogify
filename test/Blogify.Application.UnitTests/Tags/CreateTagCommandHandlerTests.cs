using Blogify.Application.Tags.CreateTag;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class CreateTagCommandHandlerTests
{
    private readonly CreateTagCommandHandler _handler;
    private readonly ITagRepository _tagRepository;

    public CreateTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new CreateTagCommandHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithUniqueName_Should_SucceedAndReturnTagId()
    {
        // Arrange
        var command = new CreateTagCommand("New Tag");
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns((Tag)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        // Verify that the repository's AddAsync method was called exactly once
        await _tagRepository.Received(1)
            .AddAsync(Arg.Is<Tag>(t => t.Name.Value == command.Name), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_Should_ReturnDuplicateNameError()
    {
        // Arrange
        var command = new CreateTagCommand("Existing Tag");
        var existingTag = Tag.Create(command.Name).Value; // Create a valid tag to be returned by the mock

        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.DuplicateName);

        // Verify that AddAsync was never called when a duplicate is found
        await _tagRepository.DidNotReceive().AddAsync(Arg.Any<Tag>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_WithInvalidName_Should_ReturnDomainValidationError(string invalidName)
    {
        // Arrange
        var command = new CreateTagCommand(invalidName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NameEmpty);

        // Verify no repository interactions occurred because validation failed early
        await _tagRepository.DidNotReceive().GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _tagRepository.DidNotReceive().AddAsync(Arg.Any<Tag>(), Arg.Any<CancellationToken>());
    }
}