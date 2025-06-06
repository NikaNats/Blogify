using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blogify.Application.Comments;
using Blogify.Application.Comments.CreateComment;
using Blogify.Application.Comments.UpdateComment;
using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.FunctionalTests.Infrastructure;
using Blogify.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Blogify.FunctionalTests.Comments;

public class CommentsControllerTests(FunctionalTestWebAppFactory factory, ITestOutputHelper testOutputHelper)
    : BaseFunctionalTest(factory), IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/comments";
    private readonly ApplicationDbContext _dbContext = factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
    private string? _accessToken;

    public async Task InitializeAsync()
    {
        _accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task DisposeAsync()
    {
        await CleanupTestData();
    }

    private async Task CleanupTestData()
    {
        var comments = await _dbContext.Set<Comment>().ToListAsync();
        _dbContext.Set<Comment>().RemoveRange(comments);
        await _dbContext.SaveChangesAsync();
    }

    private static CreateCommentCommand CreateUniqueComment(Guid postId)
    {
        return new CreateCommentCommand("This is a test comment.", Guid.NewGuid(), postId);
    }

    private async Task<(Guid Id, CommentResponse Comment)> SeedTestComment(Guid postId)
    {
        var request = CreateUniqueComment(postId);
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var commentId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());
        var comment = await GetCommentById(commentId);
        return (commentId, comment);
    }

    private async Task<CommentResponse> GetCommentById(Guid commentId)
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.ShouldNotBeNull();
        return comment;
    }

    private async Task<Guid> CreatePost()
    {
        var postTitleResult = PostTitle.Create("Test Post");
        var postContentResult = PostContent.Create("This is a valid content with more than 100 characters to ensure it passes the validation. Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
        var postExcerptResult = PostExcerpt.Create("Test Excerpt");

        if (postTitleResult.IsFailure || postContentResult.IsFailure || postExcerptResult.IsFailure)
        {
            var errors = new List<string>();
            if (postTitleResult.IsFailure) errors.Add(postTitleResult.Error.ToString());
            if (postContentResult.IsFailure) errors.Add(postContentResult.Error.ToString());
            if (postExcerptResult.IsFailure) errors.Add(postExcerptResult.Error.ToString());
            throw new InvalidOperationException($"Post creation failed due to invalid input: {string.Join(", ", errors)}");
        }

        var postTitle = postTitleResult.Value;
        var postContent = postContentResult.Value;
        var postExcerpt = postExcerptResult.Value;
        var authorId = Guid.NewGuid();

        var createPostCommand = new CreatePostCommand(postTitle, postContent, postExcerpt, authorId);
        var response = await HttpClient.PostAsJsonAsync("api/v1/posts", createPostCommand);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task CreateComment_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var postId = await CreatePost();
        var request = CreateUniqueComment(postId);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        var commentId = await response.Content.ReadFromJsonAsync<Guid>();
        var commentResponse = await GetCommentById(commentId);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        commentId.ShouldNotBe(Guid.Empty);
        commentResponse.ShouldNotBeNull();
        commentResponse.Id.ShouldBe(commentId);
        commentResponse.Content.ShouldBe(request.Content);
        commentResponse.PostId.ShouldBe(request.PostId);
        commentResponse.AuthorId.ShouldBe(request.AuthorId);
    }

    [Fact]
    public async Task CreateComment_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        var postId = await CreatePost();
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var request = CreateUniqueComment(postId);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommentById_WithValidId_ShouldReturnComment()
    {
        // Arrange
        var postId = await CreatePost();
        var (commentId, seededComment) = await SeedTestComment(postId);

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        comment.ShouldNotBeNull();
        comment.Id.ShouldBe(seededComment.Id);
        comment.Content.ShouldBe(seededComment.Content);
        comment.PostId.ShouldBe(seededComment.PostId);
        comment.AuthorId.ShouldBe(seededComment.AuthorId);
    }

    [Fact]
    public async Task GetCommentById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComment_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var postId = await CreatePost();
        var (commentId, originalComment) = await SeedTestComment(postId);
        var updateRequest = new UpdateCommentCommand(commentId, "Updated Comment", originalComment.AuthorId);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{commentId}", updateRequest);
        var updatedComment = await GetCommentById(commentId);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updatedComment.Content.ShouldBe(updateRequest.Content);
    }

    [Fact]
    public async Task UpdateComment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = new UpdateCommentCommand(invalidId, "Updated Comment", Guid.NewGuid());

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var postId = await CreatePost();
        var (commentId, seededComment) = await SeedTestComment(postId);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiEndpoint}/{commentId}")
        {
            Content = JsonContent.Create(seededComment.AuthorId)
        };

        // Act
        var deleteResponse = await HttpClient.SendAsync(request);
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiEndpoint}/{invalidId}")
        {
            Content = JsonContent.Create(authorId)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}