using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Tags;

public class GetAllTagsQueryHandlerTests
{
    private readonly GetAllTagsQueryHandler _handler;
    private readonly ITagRepository _tagRepository;

    public GetAllTagsQueryHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new GetAllTagsQueryHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WhenTagsExist_Should_ReturnListOfTagResponses()
    {
        // Arrange
        var query = new GetAllTagsQuery();
        var tags = new List<Tag>
        {
            Tag.Create("Tag 1").Value,
            Tag.Create("Tag 2").Value
        };

        _tagRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(TestPaging.Of(tags));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.Items[0].Name.ShouldBe("Tag 1");
    }

    [Fact]
    public async Task Handle_WhenNoTagsExist_Should_ReturnEmptyList()
    {
        // Arrange
        var query = new GetAllTagsQuery();
        _tagRepository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(TestPaging.Of(new List<Tag>()));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Items.ShouldBeEmpty();
    }
}