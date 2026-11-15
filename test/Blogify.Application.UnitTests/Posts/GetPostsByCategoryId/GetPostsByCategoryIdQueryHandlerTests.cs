using Blogify.Domain.Comments;
using System.Linq.Expressions;
using Blogify.Application.Posts.GetPostsByCategoryId;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.GetPostsByCategoryId;

public class GetPostsByCategoryIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepositoryMock;
    private readonly GetPostsByCategoryIdQueryHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;

    public GetPostsByCategoryIdQueryHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();

        _handler = new GetPostsByCategoryIdQueryHandler(
            _postRepositoryMock,
            _categoryRepositoryMock,
            _tagRepositoryMock, Substitute.For<ICommentRepository>());
    }

    [Fact]
    public async Task Handle_WhenPostsExistForCategory_ShouldReturnProperlyMappedResponse()
    {
        // Arrange
        var category = TestFactory.CreateCategory();
        var tag = TestFactory.CreateTag();
        var post = TestFactory.CreatePost();

        // Establish the unidirectional relationship from the Post aggregate
        post.AssignToCategory(category);
        post.AddTag(tag);

        var query = new GetPostsByCategoryIdQuery(category.Id);

        // Mock the actual methods called by the refactored handler
        _categoryRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);
        _postRepositoryMock.GetByCategoryIdAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(TestPaging.Of(new List<Post> { post }));
        _categoryRepositoryMock.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(new List<Category> { category });
        _tagRepositoryMock.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(new List<Tag> { tag });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value.Items.Single();

        response.Id.ShouldBe(post.Id);
        response.Categories.ShouldHaveSingleItem().Id.ShouldBe(category.Id);
        response.Tags.ShouldHaveSingleItem().Id.ShouldBe(tag.Id);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var query = new GetPostsByCategoryIdQuery(Guid.NewGuid());
        _categoryRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(CategoryError.NotFound);
        await _postRepositoryMock.DidNotReceive().GetByCategoryIdAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryExistsButHasNoPosts_ShouldReturnEmptyList()
    {
        // Arrange
        var category = TestFactory.CreateCategory();
        var query = new GetPostsByCategoryIdQuery(category.Id);

        _categoryRepositoryMock.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);
        _postRepositoryMock.GetByCategoryIdAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(TestPaging.Of(new List<Post>()));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty("The handler should correctly filter posts by category ID.");
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost()
        {
            var result = Post.Create(
                "A Test Post",
                new string('a', 101),
                "An excerpt for testing.",
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
            var result = Category.Create("Test Category", "A description for our test category.");
            result.IsSuccess.ShouldBeTrue();
            return result.Value;
        }
    }

    #endregion
}