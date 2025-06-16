using Blogify.Application.Tags.GetTagById;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class GetTagByIdQueryHandlerTests
{
    private readonly GetTagByIdQueryHandler _handler;
    private readonly ITagRepository _tagRepository;

    public GetTagByIdQueryHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new GetTagByIdQueryHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WhenTagExists_Should_ReturnTagResponse()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var query = new GetTagByIdQuery(tagId);
        var tag = Tag.Create("Test Tag").Value;

        _tagRepository.GetByIdAsync(tagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var response = result.Value;
        response.ShouldNotBeNull();
        response.Id.ShouldBe(tag.Id);
        response.Name.ShouldBe(tag.Name.Value);
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_Should_ReturnNotFoundError()
    {
        // Arrange
        var query = new GetTagByIdQuery(Guid.NewGuid());

        _tagRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((Tag)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);
    }
}