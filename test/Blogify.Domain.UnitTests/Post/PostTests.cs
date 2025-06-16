using Blogify.Domain.Posts;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;
using Shouldly;

namespace Blogify.Domain.UnitTests.Post;

public static class PostTests
{
    private const string ValidTitle = "A Great Post";

    private const string ValidContent =
        "This is some valid content that is definitely over one hundred characters long to satisfy all the business rules we have in place for creating a new post.";

    private const string ValidExcerpt = "A valid excerpt.";

    public class Create
    {
        [Fact]
        public void WithValidParameters_Should_SucceedAndReturnDraftPost()
        {
            // Arrange
            var authorId = Guid.NewGuid();

            // Act
            var result = Posts.Post.Create(ValidTitle, ValidContent, ValidExcerpt, authorId);

            // Assert
            result.IsSuccess.ShouldBeTrue();

            var post = result.Value;
            post.ShouldNotBeNull();
            post.Title.Value.ShouldBe(ValidTitle);
            post.Content.Value.ShouldBe(ValidContent);
            post.Excerpt.Value.ShouldBe(ValidExcerpt);
            post.AuthorId.ShouldBe(authorId);
            post.Status.ShouldBe(PublicationStatus.Draft);
            post.PublishedAt.ShouldBeNull();
            post.Slug.Value.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void WithValidParameters_Should_RaisePostCreatedEvent()
        {
            // Act
            var post = Posts.Post.Create(ValidTitle, ValidContent, ValidExcerpt, Guid.NewGuid()).Value;

            // Assert
            var domainEvent = post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostCreatedDomainEvent>();
            domainEvent.PostId.ShouldBe(post.Id);
        }

        [Fact]
        public void WithEmptyAuthorId_Should_ReturnFailure()
        {
            // Act
            var result = Posts.Post.Create(ValidTitle, ValidContent, ValidExcerpt, Guid.Empty);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(PostErrors.AuthorIdEmpty);
        }
    }

    public class Update
    {
        [Fact]
        public void WithValidChanges_Should_ChangeProperties()
        {
            // Arrange
            var post = TestFactory.CreateValidPost();
            var newTitle = "An Updated Title";
            var newContent = new string('b', 101);
            var newExcerpt = "An updated excerpt.";

            // Act
            var result = post.Update(newTitle, newContent, newExcerpt);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.Title.Value.ShouldBe(newTitle);
            post.Content.Value.ShouldBe(newContent);
            post.Excerpt.Value.ShouldBe(newExcerpt);
        }

        [Fact]
        public void WithValidChanges_Should_RaisePostUpdatedEvent()
        {
            // Arrange
            var post = TestFactory.CreateValidPost();
            post.ClearDomainEvents();

            // Act
            post.Update("New Title", ValidContent, "New Excerpt");

            // Assert
            post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostUpdatedDomainEvent>();
        }

        [Fact]
        public void WhenArchived_Should_ReturnFailure()
        {
            // Arrange
            var post = TestFactory.CreateValidPost(PublicationStatus.Archived);

            // Act
            var result = post.Update("New Title", ValidContent, "New Excerpt");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(PostErrors.CannotUpdateArchived);
        }
    }

    public class Publish
    {
        [Fact]
        public void WhenDraft_Should_SucceedAndRaiseEvent()
        {
            // Arrange
            var post = TestFactory.CreateValidPost();
            post.ClearDomainEvents();

            // Act
            var result = post.Publish();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.Status.ShouldBe(PublicationStatus.Published);
            post.PublishedAt.ShouldNotBeNull();

            var domainEvent = post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostPublishedDomainEvent>();
            domainEvent.PostId.ShouldBe(post.Id);
            domainEvent.PublishedAt.ShouldBe(post.PublishedAt.Value);
        }

        [Theory]
        [InlineData(PublicationStatus.Published, "Post.AlreadyPublished")]
        [InlineData(PublicationStatus.Archived, "Post.Publish.Archived")]
        public void WhenNotDraft_Should_ReturnFailure(PublicationStatus status, string expectedErrorCode)
        {
            // Arrange
            var post = TestFactory.CreateValidPost(status);

            // Act
            var result = post.Publish();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe(expectedErrorCode);
        }
    }

    public class Archive
    {
        [Theory]
        [InlineData(PublicationStatus.Draft)]
        [InlineData(PublicationStatus.Published)]
        public void WhenNotArchived_Should_SucceedAndRaiseEvent(PublicationStatus initialStatus)
        {
            // Arrange
            var post = TestFactory.CreateValidPost(initialStatus);
            post.ClearDomainEvents();

            // Act
            var result = post.Archive();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.Status.ShouldBe(PublicationStatus.Archived);
            post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostArchivedDomainEvent>();
        }

        [Fact]
        public void WhenAlreadyArchived_Should_SucceedAndBeIdempotent()
        {
            // Arrange
            var post = TestFactory.CreateValidPost(PublicationStatus.Archived);
            post.ClearDomainEvents();

            // Act
            var result = post.Archive();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.DomainEvents.ShouldBeEmpty();
        }
    }

    public class CategoryManagement
    {
        [Fact]
        public void AssignToCategory_WhenNotAssigned_Should_AddIdAndRaiseEvent()
        {
            // Arrange
            var post = TestFactory.CreateValidPost();
            var category = TestFactory.CreateValidCategory();
            post.ClearDomainEvents();

            // Act
            var result = post.AssignToCategory(category);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.CategoryIds.ShouldContain(category.Id);

            var domainEvent = post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostCategoryAddedDomainEvent>();
            domainEvent.CategoryId.ShouldBe(category.Id);
        }

        [Fact]
        public void RemoveFromCategory_WhenAssigned_Should_RemoveIdAndRaiseEvent()
        {
            // Arrange
            var post = TestFactory.CreateValidPost();
            var category = TestFactory.CreateValidCategory();
            post.AssignToCategory(category);
            post.ClearDomainEvents();

            // Act
            var result = post.RemoveFromCategory(category);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.CategoryIds.ShouldNotContain(category.Id);
            post.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PostCategoryRemovedDomainEvent>();
        }
    }

    public class CommentManagement
    {
        [Fact]
        public void AddComment_WhenPublished_Should_SucceedAndAddComment()
        {
            // Arrange
            var post = TestFactory.CreateValidPost(PublicationStatus.Published);
            var authorId = Guid.NewGuid();
            var commentContent = "This is a fantastic post!";

            // Act
            var result = post.AddComment(commentContent, authorId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            post.Comments.ShouldHaveSingleItem().Content.Value.ShouldBe(commentContent);
        }

        [Fact]
        public void AddComment_WhenNotPublished_Should_ReturnFailure()
        {
            // Arrange
            var post = TestFactory.CreateValidPost(PublicationStatus.Draft);

            // Act
            var result = post.AddComment("This comment should fail.", Guid.NewGuid());

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(PostErrors.CommentToUnpublishedPost);
            post.Comments.ShouldBeEmpty();
        }
    }

    /// <summary>
    ///     A clean, reusable factory for creating valid domain entities for tests.
    ///     It hides the complexity of creation and returns a valid entity directly.
    /// </summary>
    private static class TestFactory
    {
        internal static Posts.Post CreateValidPost(PublicationStatus status = PublicationStatus.Draft)
        {
            var result = Posts.Post.Create(ValidTitle, ValidContent, ValidExcerpt, Guid.NewGuid());

            if (result.IsFailure)
                throw new InvalidOperationException(
                    $"Failed to create valid Post for test setup: {result.Error.Description}");

            var post = result.Value;
            if (status == PublicationStatus.Published) post.Publish();
            if (status == PublicationStatus.Archived) post.Archive();

            return post;
        }

        internal static Categories.Category CreateValidCategory()
        {
            var result = Categories.Category.Create("Test Category", "A description.");
            if (result.IsFailure) throw new InvalidOperationException("Failed to create Category for test.");
            return result.Value;
        }

        internal static Tag CreateValidTag()
        {
            var result = Tag.Create("Test Tag");
            if (result.IsFailure) throw new InvalidOperationException("Failed to create Tag for test.");
            return result.Value;
        }
    }
}