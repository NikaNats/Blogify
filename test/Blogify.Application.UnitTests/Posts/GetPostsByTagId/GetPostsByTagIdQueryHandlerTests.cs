using System.Linq.Expressions;
using Blogify.Application.Posts.GetPostsByTagId;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetPostsByTagId;

public class GetPostsByTagIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;

    // --- CHANGED: Added all required repository mocks ---
    private readonly GetPostsByTagIdQueryHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;

    public GetPostsByTagIdQueryHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();

        // --- CHANGED: Inject all dependencies into the handler ---
        _handler = new GetPostsByTagIdQueryHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _tagRepositoryMock);
    }

    [Fact]
    public async Task Handle_WhenPostsExistForTag_ShouldReturnProperlyMappedResponse()
    {
        // Arrange
        var tag = TestFactory.CreateTag();
        var category = TestFactory.CreateCategory();
        var post = TestFactory.CreatePost();

        // --- FIXED: Correctly establish the unidirectional relationship ---
        post.AddTag(tag);
        post.AssignToCategory(category);

        var query = new GetPostsByTagIdQuery(tag.Id);

        // --- FIXED: Mock the actual methods called by the handler ---
        _tagRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Tag, bool>>>()).Returns(true);
        _postRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Post> { post });
        _tagRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Tag> { tag });
        _categoryRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Category> { category });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value.Single();

        response.Id.ShouldBe(post.Id);
        response.Tags.ShouldHaveSingleItem().Id.ShouldBe(tag.Id);
        response.Categories.ShouldHaveSingleItem().Id.ShouldBe(category.Id);
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPostsByTagIdQuery(Guid.NewGuid());
        _tagRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Tag, bool>>>()).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
        await _postRepositoryMock.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagExistsButHasNoPosts_ShouldReturnEmptyList()
    {
        // Arrange
        var tag = TestFactory.CreateTag();
        var query = new GetPostsByTagIdQuery(tag.Id);

        _tagRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Tag, bool>>>()).Returns(true);
        // Return a post that is NOT associated with the tag
        _postRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Post> { TestFactory.CreatePost() });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost()
        {
            var result = Post.Create(
                "Test Post",
                new string('a', 101),
                "An excerpt.",
                Guid.NewGuid());
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        internal static Tag CreateTag()
        {
            var result = Tag.Create("Test Tag");
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        internal static Category CreateCategory()
        {
            var result = Category.Create("Test Category", "A description");
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}