// using Blogify.Application.Categories.AddPostToCategoryCommand;
// using Blogify.Domain.Abstractions;
// using Blogify.Domain.Categories;
// using Blogify.Domain.Posts;
// using FluentAssertions;
// using NSubstitute;
//
// namespace Blogify.Application.UnitTests.Categories.AddPost;
//
// public class AddPostToCategoryCommandHandlerTests
// {
//     private readonly AddPostToCategoryCommandHandler _handler;
//     private readonly ICategoryRepository _categoryRepositoryMock;
//     private readonly IPostRepository _postRepositoryMock;
//
//     public AddPostToCategoryCommandHandlerTests()
//     {
//         _categoryRepositoryMock = Substitute.For<ICategoryRepository>();
//         _postRepositoryMock = Substitute.For<IPostRepository>();
//
//         _handler = new AddPostToCategoryCommandHandler(_categoryRepositoryMock, _postRepositoryMock);
//     }
//
//     [Fact]
//     public async Task Handle_Should_ReturnFailure_WhenCategoryIsNull()
//     {
//         // Arrange
//         var command = new AddPostToCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
//
//         _categoryRepositoryMock
//             .GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
//             .Returns((Category?)null);
//
//         // Act
//         Result result = await _handler.Handle(command, default);
//
//         // Assert
//         result.Error.Should().Be(Error.NotFound("Category.NotFound", "Category not found."));
//     }
//
//     [Fact]
//     public async Task Handle_Should_ReturnFailure_WhenPostIsNull()
//     {
//         // Arrange
//         var command = new AddPostToCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
//         var category = Category.Create("Test Category");
//
//         _categoryRepositoryMock
//             .GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
//             .Returns(category);
//
//         _postRepositoryMock
//             .GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
//             .Returns((Post?)null);
//
//         // Act
//         Result result = await _handler.Handle(command, default);
//
//         // Assert
//         result.Error.Should().Be(Error.NotFound("Post.NotFound", "Post not found."));
//     }
//
//     [Fact]
//     public async Task Handle_Should_ReturnFailure_WhenAddingPostFails()
//     {
//         // Arrange
//         var command = new AddPostToCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
//         var category = Category.Create("Test Category");
//         var post = Post.Create("Test Post", "Test Content");
//
//         _categoryRepositoryMock
//             .GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
//             .Returns(category);
//
//         _postRepositoryMock
//             .GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
//             .Returns(post);
//
//         category.AddPost(post).Returns(Result.Failure(Error.Failure("Category.PostAddFailed", "Failed to add post to category.")));
//
//         // Act
//         Result result = await _handler.Handle(command, default);
//
//         // Assert
//         result.Error.Should().Be(Error.Failure("Category.PostAddFailed", "Failed to add post to category."));
//     }
//
//     [Fact]
//     public async Task Handle_Should_ReturnSuccess_WhenPostIsAddedToCategory()
//     {
//         // Arrange
//         var command = new AddPostToCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
//         var category = Category.Create("Test Category");
//         var post = Post.Create("Test Post", "Test Content");
//
//         _categoryRepositoryMock
//             .GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
//             .Returns(category);
//
//         _postRepositoryMock
//             .GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
//             .Returns(post);
//
//         category.AddPost(post).Returns(Result.Success());
//
//         // Act
//         Result result = await _handler.Handle(command, default);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//     }
//
//     [Fact]
//     public async Task Handle_Should_CallRepository_WhenPostIsAddedToCategory()
//     {
//         // Arrange
//         var command = new AddPostToCategoryCommand(Guid.NewGuid(), Guid.NewGuid());
//         var category = Category.Create("Test Category");
//         var post = Post.Create("Test Post", "Test Content");
//
//         _categoryRepositoryMock
//             .GetByIdAsync(command.CategoryId, Arg.Any<CancellationToken>())
//             .Returns(category);
//
//         _postRepositoryMock
//             .GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
//             .Returns(post);
//
//         category.AddPost(post).Returns(Result.Success());
//
//         // Act
//         Result result = await _handler.Handle(command, default);
//
//         // Assert
//         await _categoryRepositoryMock.Received(1).UpdateAsync(category, Arg.Any<CancellationToken>());
//     }
// }