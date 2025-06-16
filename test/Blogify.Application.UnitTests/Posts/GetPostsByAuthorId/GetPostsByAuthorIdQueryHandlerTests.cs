using Blogify.Application.Posts.GetPostsByAuthorId;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetPostsByAuthorId;

public class GetPostsByAuthorIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;

    // --- CHANGED: Added all required repository mocks ---
    private readonly GetPostsByAuthorIdQueryHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;

    public GetPostsByAuthorIdQueryHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();

        // --- CHANGED: Inject all dependencies into the handler ---
        _handler = new GetPostsByAuthorIdQueryHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _tagRepositoryMock);
    }

    [Fact]
    public async Task Handle_WhenNoPostsExistForAuthor_ShouldReturnEmptyList()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        _postRepositoryMock.GetByAuthorIdAsync(authorId, Arg.Any<CancellationToken>()).Returns(new List<Post>());

        // Act
        var result = await _handler.Handle(new GetPostsByAuthorIdQuery(authorId), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPostsExistForAuthor_ShouldReturnMappedResponses()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var category = TestFactory.CreateCategory();
        var tag = TestFactory.CreateTag();

        var post1 = TestFactory.CreatePost(authorId);
        post1.AssignToCategory(category);
        post1.AddTag(tag);

        var post2 = TestFactory.CreatePost(authorId);
        // post2 has no tags or categories

        var posts = new List<Post> { post1, post2 };

        // --- FIXED: Mock all repository calls made by the handler ---
        _postRepositoryMock.GetByAuthorIdAsync(authorId, Arg.Any<CancellationToken>()).Returns(posts);
        // Mock the batch-loading calls
        _categoryRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Category> { category });
        _tagRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Tag> { tag });

        // Act
        var result = await _handler.Handle(new GetPostsByAuthorIdQuery(authorId), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);

        var responseForPost1 = result.Value.Single(r => r.Id == post1.Id);
        responseForPost1.AuthorId.ShouldBe(authorId);
        responseForPost1.Categories.ShouldHaveSingleItem().Id.ShouldBe(category.Id);
        responseForPost1.Tags.ShouldHaveSingleItem().Id.ShouldBe(tag.Id);

        var responseForPost2 = result.Value.Single(r => r.Id == post2.Id);
        responseForPost2.AuthorId.ShouldBe(authorId);
        responseForPost2.Categories.ShouldBeEmpty();
        responseForPost2.Tags.ShouldBeEmpty();
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost(Guid authorId)
        {
            var result = Post.Create(
                "Test Post",
                new string('a', 101),
                "An excerpt.",
                authorId);
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        internal static Category CreateCategory()
        {
            var result = Category.Create("Test Category", "A description");
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }

        internal static Tag CreateTag()
        {
            var result = Tag.Create("Test Tag");
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}