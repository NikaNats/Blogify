using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Domain.UnitTests.Post
{
    public class PostTests
    {
        [Fact]
        public void Create_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var title = PostTitle.Create("Test Title").Value;
            var content = PostContent.Create(new string('a', 100)).Value; // Valid content
            var excerpt = PostExcerpt.Create("Test Excerpt").Value;
            var authorId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            // Act
            var result = Posts.Post.Create(title, content, excerpt, authorId, categoryId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(title, result.Value.Title);
            Assert.Equal(content, result.Value.Content);
            Assert.Equal(excerpt, result.Value.Excerpt);
            Assert.Equal(authorId, result.Value.AuthorId);
            Assert.Equal(categoryId, result.Value.CategoryId);
        }

        [Fact]
        public void Create_NullTitle_ReturnsFailure()
        {
            // Arrange
            var content = PostContent.Create(new string('a', 100)).Value;
            var excerpt = PostExcerpt.Create("Test Excerpt").Value;
            var authorId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            // Act
            var result = Posts.Post.Create(null, content, excerpt, authorId, categoryId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Post.Title", result.Error.Code);
        }

        [Fact]
        public void Update_ValidInput_UpdatesPost()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            var newTitle = PostTitle.Create("Updated Title").Value;
            var newContent = PostContent.Create(new string('b', 100)).Value;
            var newExcerpt = PostExcerpt.Create("Updated Excerpt").Value;
            var newCategoryId = Guid.NewGuid();

            // Act
            post.Update(newTitle, newContent, newExcerpt, newCategoryId);

            // Assert
            Assert.Equal(newTitle, post.Title);
            Assert.Equal(newContent, post.Content);
            Assert.Equal(newExcerpt, post.Excerpt);
            Assert.Equal(newCategoryId, post.CategoryId);
            Assert.NotNull(post.UpdatedAt);
        }

        [Fact]
        public void Publish_ValidPost_PublishesPost()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            // Act
            post.Publish();

            // Assert
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.NotNull(post.PublishedAt);
            Assert.NotNull(post.UpdatedAt);
        }

        [Fact]
        public void Publish_AlreadyPublished_ReturnsFailure()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            post.Publish();

            // Act
            post.Publish();

            // Assert
            Assert.Equal(PostStatus.Published, post.Status);
        }

        [Fact]
        public void Archive_ValidPost_ArchivesPost()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            // Act
            post.Archive();

            // Assert
            Assert.Equal(PostStatus.Archived, post.Status);
            Assert.NotNull(post.UpdatedAt);
        }

        [Fact]
        public void Archive_AlreadyArchived_ReturnsFailure()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            post.Archive();

            // Act
            post.Archive();

            // Assert
            Assert.Equal(PostStatus.Archived, post.Status);
        }

        [Fact]
        public void AddComment_ValidComment_AddsComment()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            post.Publish();
            var commentContent = "Test Comment";
            var authorId = Guid.NewGuid();

            // Act
            post.AddComment(commentContent, authorId);

            // Assert
            Assert.Single(post.Comments);
            Assert.Equal(commentContent, post.Comments.First().Content);
            Assert.Equal(authorId, post.Comments.First().AuthorId);
        }

        [Fact]
        public void AddComment_UnpublishedPost_ReturnsFailure()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            var commentContent = "Test Comment";
            var authorId = Guid.NewGuid();

            // Act
            post.AddComment(commentContent, authorId);

            // Assert
            Assert.Empty(post.Comments);
        }

        [Fact]
        public void AddTag_ValidTag_AddsTag()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            var tag = Tag.Create("Test Tag").Value;

            // Act
            post.AddTag(tag);

            // Assert
            Assert.Single(post.Tags);
            Assert.Equal(tag, post.Tags.First());
        }

        [Fact]
        public void RemoveTag_ExistingTag_RemovesTag()
        {
            // Arrange
            var post = Posts.Post.Create(
                PostTitle.Create("Test Title").Value,
                PostContent.Create(new string('a', 100)).Value,
                PostExcerpt.Create("Test Excerpt").Value,
                Guid.NewGuid(),
                Guid.NewGuid()).Value;

            var tag = Tag.Create("Test Tag").Value;
            post.AddTag(tag);

            // Act
            post.RemoveTag(tag);

            // Assert
            Assert.Empty(post.Tags);
        }
    }
}