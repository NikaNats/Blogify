using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Domain.UnitTests.Post;

public class PostDomainTests
{
    #region Constants and Helper Methods

    private const string ValidTitle = "Test Title";

    private const string ValidContent =
        "This is a valid post content that meets the minimum length requirement of 100 characters. It should be long enough to pass validation.";

    private const string ValidExcerpt = "Test Excerpt";
    private const int MinContentLength = 100;

    private Posts.Post CreateValidPost(Guid? authorId = null, PublicationStatus status = PublicationStatus.Draft)
    {
        var authorIdValue = authorId ?? Guid.NewGuid();
        var titleResult = PostTitle.Create(ValidTitle);
        var contentResult = PostContent.Create(ValidContent);
        var excerptResult = PostExcerpt.Create(ValidExcerpt);

        Assert.True(titleResult.IsSuccess, $"Failed to create title: {titleResult.Error.Description}");
        Assert.True(contentResult.IsSuccess, $"Failed to create content: {contentResult.Error.Description}");
        Assert.True(excerptResult.IsSuccess, $"Failed to create excerpt: {excerptResult.Error.Description}");

        var postResult = Posts.Post.Create(titleResult.Value, contentResult.Value, excerptResult.Value, authorIdValue);
        Assert.True(postResult.IsSuccess, $"Failed to create post: {postResult.Error.Description}");

        var post = postResult.Value;
        if (status == PublicationStatus.Published)
            post.Publish();
        else if (status == PublicationStatus.Archived)
            post.Archive();

        return post;
    }

    #endregion

    #region Post Creation Tests

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var authorId = Guid.NewGuid();
        var post = CreateValidPost(authorId);

        Assert.NotNull(post);
        Assert.Equal(ValidTitle, post.Title.Value);
        Assert.Equal(ValidContent, post.Content.Value);
        Assert.Equal(ValidExcerpt, post.Excerpt.Value);
        Assert.Equal(authorId, post.AuthorId);
        Assert.Equal(PublicationStatus.Draft, post.Status);
        Assert.NotNull(post.Slug.Value);
        Assert.True(DateTimeOffset.UtcNow >= post.CreatedAt);
        Assert.Equal(post.CreatedAt, post.LastModifiedAt);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullTitle_ReturnsFailure()
    {
        var content = PostContent.Create(ValidContent).Value;
        var excerpt = PostExcerpt.Create(ValidExcerpt).Value;
        var authorId = Guid.NewGuid();
        var result = Posts.Post.Create(null!, content, excerpt, authorId);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Title.Null", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullContent_ReturnsFailure()
    {
        var title = PostTitle.Create(ValidTitle).Value;
        PostContent content = null!;
        var excerpt = PostExcerpt.Create(ValidExcerpt).Value;
        var authorId = Guid.NewGuid();
        var result = Posts.Post.Create(title, content, excerpt, authorId);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Content.Null", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullExcerpt_ReturnsFailure()
    {
        var title = PostTitle.Create(ValidTitle).Value;
        var content = PostContent.Create(ValidContent).Value;
        PostExcerpt excerpt = null!;
        var authorId = Guid.NewGuid();
        var result = Posts.Post.Create(title, content, excerpt, authorId);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Excerpt.Null", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_EmptyAuthorId_ReturnsFailure()
    {
        var title = PostTitle.Create(ValidTitle).Value;
        var content = PostContent.Create(ValidContent).Value;
        var excerpt = PostExcerpt.Create(ValidExcerpt).Value;
        var result = Posts.Post.Create(title, content, excerpt, Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.AuthorId.Empty", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ValidInput_GeneratesSlug()
    {
        var authorId = Guid.NewGuid();
        var title = PostTitle.Create("Test Title").Value;
        var content = PostContent.Create(ValidContent).Value;
        var excerpt = PostExcerpt.Create(ValidExcerpt).Value;
        var result = Posts.Post.Create(title, content, excerpt, authorId);

        Assert.True(result.IsSuccess);
        Assert.Equal("test-title", result.Value.Slug.Value);
    }

    #endregion

    #region Post Update Tests

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ValidInput_UpdatesPost()
    {
        var post = CreateValidPost();
        var newTitle = PostTitle.Create("Updated Title").Value;
        var newContent = PostContent.Create(new string('b', MinContentLength)).Value;
        var newExcerpt = PostExcerpt.Create("Updated Excerpt").Value;
        var result = post.Update(newTitle, newContent, newExcerpt);

        Assert.True(result.IsSuccess);
        Assert.Equal(newTitle, post.Title);
        Assert.Equal(newContent, post.Content);
        Assert.Equal(newExcerpt, post.Excerpt);
        Assert.True(post.LastModifiedAt > post.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ArchivedPost_ReturnsFailure()
    {
        var post = CreateValidPost(status: PublicationStatus.Archived);
        var newTitle = PostTitle.Create("Updated Title").Value;
        var newContent = PostContent.Create(ValidContent).Value;
        var newExcerpt = PostExcerpt.Create("Updated Excerpt").Value;
        var result = post.Update(newTitle, newContent, newExcerpt);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Update.Archived", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ValidInput_UpdatesLastModifiedAt()
    {
        var post = CreateValidPost();
        var originalLastModifiedAt = post.LastModifiedAt;
        var newTitle = PostTitle.Create("Updated Title").Value;
        var newContent = PostContent.Create(ValidContent).Value;
        var newExcerpt = PostExcerpt.Create("Updated Excerpt").Value;
        var result = post.Update(newTitle, newContent, newExcerpt);

        Assert.True(result.IsSuccess);
        Assert.True(post.LastModifiedAt > originalLastModifiedAt);
    }

    #endregion

    #region Post Publishing Tests

    [Fact]
    [Trait("Category", "Publishing")]
    public void Publish_DraftPost_PublishesPost()
    {
        var post = CreateValidPost();
        var result = post.Publish();

        Assert.True(result.IsSuccess);
        Assert.Equal(PublicationStatus.Published, post.Status);
        Assert.NotNull(post.PublishedAt);
        Assert.True(post.PublishedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    [Trait("Category", "Publishing")]
    public void Publish_AlreadyPublishedPost_ReturnsFailure()
    {
        var post = CreateValidPost(status: PublicationStatus.Published);
        var result = post.Publish();

        Assert.True(result.IsFailure);
        Assert.Equal("Post.AlreadyPublished", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Publishing")]
    public void Publish_ArchivedPost_ReturnsFailure()
    {
        var post = CreateValidPost(status: PublicationStatus.Archived);
        var result = post.Publish();

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Publish.Archived", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Publishing")]
    public void Publish_DraftPost_UpdatesLastModifiedAt()
    {
        var post = CreateValidPost();
        var originalLastModifiedAt = post.LastModifiedAt;
        var result = post.Publish();

        Assert.True(result.IsSuccess);
        Assert.True(post.LastModifiedAt > originalLastModifiedAt);
    }

    #endregion

    #region Post Archiving Tests

    [Fact]
    [Trait("Category", "Archiving")]
    public void Archive_DraftPost_ArchivesPost()
    {
        var post = CreateValidPost();
        var result = post.Archive();

        Assert.True(result.IsSuccess);
        Assert.Equal(PublicationStatus.Archived, post.Status);
    }

    [Fact]
    [Trait("Category", "Archiving")]
    public void Archive_PublishedPost_ArchivesPost()
    {
        var post = CreateValidPost(status: PublicationStatus.Published);
        var result = post.Archive();

        Assert.True(result.IsSuccess);
        Assert.Equal(PublicationStatus.Archived, post.Status);
    }

    [Fact]
    [Trait("Category", "Archiving")]
    public void Archive_AlreadyArchivedPost_NoChange()
    {
        var post = CreateValidPost(status: PublicationStatus.Archived);
        var result = post.Archive();

        Assert.True(result.IsSuccess);
        Assert.Equal(PublicationStatus.Archived, post.Status);
    }

    [Fact]
    [Trait("Category", "Archiving")]
    public void Archive_DraftPost_UpdatesLastModifiedAt()
    {
        var post = CreateValidPost();
        var originalLastModifiedAt = post.LastModifiedAt;
        var result = post.Archive();

        Assert.True(result.IsSuccess);
        Assert.True(post.LastModifiedAt > originalLastModifiedAt);
    }

    #endregion

    #region Post Comment Tests

    [Fact]
    [Trait("Category", "Comments")]
    public void AddComment_PublishedPost_AddsComment()
    {
        var post = CreateValidPost(status: PublicationStatus.Published);
        var commentContent = "Test Comment";
        var authorId = Guid.NewGuid();
        var result = post.AddComment(commentContent, authorId);

        Assert.True(result.IsSuccess);
        Assert.Single(post.Comments);
        Assert.Equal(commentContent, post.Comments.First().Content.Value);
        Assert.Equal(authorId, post.Comments.First().AuthorId);
    }

    [Fact]
    [Trait("Category", "Comments")]
    public void AddComment_PublishedPost_UpdatesLastModifiedAt()
    {
        var post = CreateValidPost(status: PublicationStatus.Published);
        var originalLastModifiedAt = post.LastModifiedAt;
        var commentContent = "Test Comment";
        var authorId = Guid.NewGuid();
        var result = post.AddComment(commentContent, authorId);

        Assert.True(result.IsSuccess);
        Assert.True(post.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "Comments")]
    public void AddComment_UnpublishedPost_ReturnsFailure()
    {
        var post = CreateValidPost();
        var commentContent = "Test Comment";
        var authorId = Guid.NewGuid();
        var result = post.AddComment(commentContent, authorId);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.CommentToUnpublishedPost.Code, result.Error.Code);
        Assert.Empty(post.Comments);
    }

    [Fact]
    [Trait("Category", "Comments")]
    public void Comments_CollectionIsImmutable()
    {
        var post = CreateValidPost(status: PublicationStatus.Published);
        var comments = post.Comments as ICollection<Comment>;
        var commentResult = Comment.Create("Test", Guid.NewGuid(), post.Id);

        Assert.Throws<NotSupportedException>(() => comments!.Add(commentResult.Value));
    }

    #endregion

    #region Post Tag Tests

    [Fact]
    [Trait("Category", "Tags")]
    public void AddTag_ValidTag_AddsTag()
    {
        var post = CreateValidPost();
        var tag = Tag.Create("Test Tag").Value;
        var result = post.AddTag(tag);

        Assert.True(result.IsSuccess);
        Assert.Single(post.Tags);
        Assert.Equal(tag, post.Tags.First());
    }

    [Fact]
    [Trait("Category", "Tags")]
    public void RemoveTag_ExistingTag_RemovesTag()
    {
        var post = CreateValidPost();
        var tag = Tag.Create("Test Tag").Value;
        post.AddTag(tag);
        var result = post.RemoveTag(tag);

        Assert.True(result.IsSuccess);
        Assert.Empty(post.Tags);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public void RemoveTag_NonExistentTag_NoChange()
    {
        var post = CreateValidPost();
        var tag = Tag.Create("Non-existent Tag").Value;
        var result = post.RemoveTag(tag);

        Assert.True(result.IsSuccess);
        Assert.Empty(post.Tags);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public void AddTag_DuplicateTag_NoChange()
    {
        var post = CreateValidPost();
        var tag = Tag.Create("Test Tag").Value;
        post.AddTag(tag);
        var result = post.AddTag(tag);

        Assert.True(result.IsSuccess);
        Assert.Single(post.Tags);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public void Tags_CollectionIsImmutable()
    {
        var post = CreateValidPost();
        var tags = post.Tags as ICollection<Tag>;

        Assert.Throws<NotSupportedException>(() => tags!.Add(Tag.Create("Test").Value));
    }

    #endregion

    #region Post Category Tests

    [Fact]
    [Trait("Category", "Categories")]
    public void AddCategory_ValidCategory_AddsCategory()
    {
        var post = CreateValidPost();
        var category = Categories.Category.Create("Test Category", "Description").Value;
        var result = post.AddCategory(category);

        Assert.True(result.IsSuccess);
        Assert.Single(post.Categories);
        Assert.Equal(category, post.Categories.First());
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void RemoveCategory_ExistingCategory_RemovesCategory()
    {
        var post = CreateValidPost();
        var category = Categories.Category.Create("Test Category", "Description").Value;
        post.AddCategory(category);
        var result = post.RemoveCategory(category);

        Assert.True(result.IsSuccess);
        Assert.Empty(post.Categories);
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void RemoveCategory_NonExistentCategory_NoChange()
    {
        var post = CreateValidPost();
        var category = Categories.Category.Create("Non-existent Category", "Description").Value;
        var result = post.RemoveCategory(category);

        Assert.True(result.IsSuccess);
        Assert.Empty(post.Categories);
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void Categories_CollectionIsImmutable()
    {
        var post = CreateValidPost();
        var categories = post.Categories as ICollection<Categories.Category>;

        Assert.Throws<NotSupportedException>(() =>
            categories!.Add(Categories.Category.Create("Test", "Description").Value));
    }

    #endregion

    #region Post Domain Event Tests

    [Fact]
    [Trait("Category", "Events")]
    public void Create_RaisesPostCreatedDomainEvent()
    {
        var authorId = Guid.NewGuid();
        var post = CreateValidPost(authorId);
        var domainEvent = post.DomainEvents.OfType<PostCreatedDomainEvent>().SingleOrDefault();

        Assert.NotNull(domainEvent);
        Assert.Equal(post.Id, domainEvent.PostId);
        Assert.Equal(ValidTitle, domainEvent.PostTitle);
        Assert.Equal(authorId, domainEvent.AuthorId);
    }

    [Fact]
    [Trait("Category", "Events")]
    public void Publish_RaisesPostPublishedDomainEvent()
    {
        var post = CreateValidPost();
        post.ClearDomainEvents();
        post.Publish();
        var domainEvent = post.DomainEvents.OfType<PostPublishedDomainEvent>().SingleOrDefault();

        Assert.NotNull(domainEvent);
        Assert.Equal(post.Id, domainEvent.PostId);
        Assert.Equal(ValidTitle, domainEvent.PostTitle);
        Assert.Equal(post.AuthorId, domainEvent.AuthorId);
    }

    #endregion

    #region Post Equality Tests

    [Fact]
    [Trait("Category", "Equality")]
    public void Post_IsEqualToItself()
    {
        var post = CreateValidPost();

        Assert.Equal(post, post);
        Assert.True(post.Equals(post));
    }

    [Fact]
    [Trait("Category", "Equality")]
    public void Posts_WithDifferentIds_AreNotEqual()
    {
        var post1 = CreateValidPost();
        var post2 = CreateValidPost();

        Assert.NotEqual(post1, post2);
        Assert.False(post1.Equals(post2));
    }

    #endregion

    #region PostTitle Tests

    [Fact]
    [Trait("Category", "PostTitle")]
    public void Create_ValidTitle_ReturnsSuccess()
    {
        var title = "Valid Title";
        var result = PostTitle.Create(title);

        Assert.True(result.IsSuccess);
        Assert.Equal(title, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "PostTitle")]
    public void Create_EmptyTitle_ReturnsFailure()
    {
        var title = "";
        var result = PostTitle.Create(title);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.TitleEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "PostTitle")]
    public void Create_TitleTooLong_ReturnsFailure()
    {
        var title = new string('a', 201);
        var result = PostTitle.Create(title);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.TitleTooLong, result.Error);
    }

    #endregion

    #region PostContent Tests

    [Fact]
    [Trait("Category", "PostContent")]
    public void Create_ValidContent_ReturnsSuccess()
    {
        const string content = "This is a valid content with more than 100 characters. " +
                               "This is additional text to ensure the content length is greater than 100 characters.";
        var result = PostContent.Create(content);

        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "PostContent")]
    public void Create_EmptyContent_ReturnsFailure()
    {
        var content = "";
        var result = PostContent.Create(content);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ContentEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "PostContent")]
    public void Create_ContentTooShort_ReturnsFailure()
    {
        var content = "Short";
        var result = PostContent.Create(content);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ContentTooShort, result.Error);
    }

    #endregion

    #region PostExcerpt Tests

    [Fact]
    [Trait("Category", "PostExcerpt")]
    public void Create_ValidExcerpt_ReturnsSuccess()
    {
        var excerpt = "This is a valid excerpt.";
        var result = PostExcerpt.Create(excerpt);

        Assert.True(result.IsSuccess);
        Assert.Equal(excerpt, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "PostExcerpt")]
    public void Create_EmptyExcerpt_ReturnsFailure()
    {
        var excerpt = "";
        var result = PostExcerpt.Create(excerpt);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ExcerptEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "PostExcerpt")]
    public void Create_ExcerptTooLong_ReturnsFailure()
    {
        var excerpt = new string('a', 501);
        var result = PostExcerpt.Create(excerpt);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ExcerptTooLong, result.Error);
    }

    #endregion

    #region PostSlug Tests

    [Fact]
    [Trait("Category", "PostSlug")]
    public void Create_ValidSlugTitle_ReturnsSuccess()
    {
        var title = "Valid Title";
        var result = PostSlug.Create(title);

        Assert.True(result.IsSuccess);
        Assert.Equal("valid-title", result.Value.Value);
    }

    [Fact]
    [Trait("Category", "PostSlug")]
    public void Create_EmptySlugTitle_ReturnsFailure()
    {
        var title = "";
        var result = PostSlug.Create(title);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.SlugEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "PostSlug")]
    public void Create_SlugTitleTooLong_ReturnsFailure()
    {
        var title = new string('a', 201);
        var result = PostSlug.Create(title);

        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.SlugTooLong, result.Error);
    }

    #endregion
}