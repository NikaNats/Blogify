using Blogify.Application.Posts.GetPostById;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetPostById;

public class GetPostByIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly GetPostByIdQueryHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;

    public GetPostByIdQueryHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();

        _handler = new GetPostByIdQueryHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _tagRepositoryMock);
    }

    [Fact]
    public async Task Handle_WhenPostIsNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var query = new GetPostByIdQuery(Guid.NewGuid());
        _postRepositoryMock
            .GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPostExists_ShouldReturnCorrectlyMappedResponse()
    {
        // Arrange
        // Create the entities that will be associated with the post.
        var post = TestFactory.CreatePost();
        var category = TestFactory.CreateCategory("Tech");
        var tag = TestFactory.CreateTag("dotnet");

        // This "distractor" entity should NOT appear in the final result.
        // This ensures we are testing the handler's filtering logic.
        var unrelatedCategory = TestFactory.CreateCategory("Lifestyle");

        post.AssignToCategory(category);
        post.AddTag(tag);

        var query = new GetPostByIdQuery(post.Id);

        // Mock the repository calls to simulate the data layer.
        _postRepositoryMock.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(post);

        // The handler calls GetAllAsync, so we mock it to return a list containing
        // both the relevant category and the distractor.
        _categoryRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category> { category, unrelatedCategory });

        _tagRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tag> { tag });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var response = result.Value;
        response.Id.ShouldBe(post.Id);
        response.Title.ShouldBe(post.Title.Value);
        response.Comments.ShouldBeEmpty();

        // Verify that the response contains ONLY the assigned category.
        response.Categories.ShouldHaveSingleItem();
        response.Categories.Single().Id.ShouldBe(category.Id);
        response.Categories.Single().Name.ShouldBe(category.Name.Value);

        // Verify that the response contains ONLY the assigned tag.
        response.Tags.ShouldHaveSingleItem();
        response.Tags.Single().Id.ShouldBe(tag.Id);
    }

    [Fact]
    public async Task Handle_WhenPostExistsWithNoCategoriesOrTags_ShouldReturnEmptyCollections()
    {
        // Arrange
        // Create a post with no assigned categories or tags.
        var post = TestFactory.CreatePost();
        var query = new GetPostByIdQuery(post.Id);

        _postRepositoryMock.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(post);

        // The repositories might still have data, but none of it is related to our post.
        _categoryRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category> { TestFactory.CreateCategory("Some Category") });
        _tagRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Tag> { TestFactory.CreateTag("Some Tag") });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var response = result.Value;
        response.Id.ShouldBe(post.Id);
        response.Categories.ShouldBeEmpty();
        response.Tags.ShouldBeEmpty();
    }

    /// <summary>
    ///     A clean, reusable factory for creating valid domain entities for tests.
    ///     It does not contain assertions, as its only job is to provide test data.
    /// </summary>
    private static class TestFactory
    {
        internal static Post CreatePost()
        {
            return Post.Create(
                "A Test Post",
                new string('a', 101), // content
                "An excerpt for testing.",
                Guid.NewGuid() // authorId
            ).Value;
        }

        internal static Category CreateCategory(string name)
        {
            return Category.Create(name, "A description for " + name).Value;
        }

        internal static Tag CreateTag(string name)
        {
            return Tag.Create(name).Value;
        }
    }
}