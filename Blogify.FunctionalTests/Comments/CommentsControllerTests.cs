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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        return new CreateCommentCommand("This is a test comment.", Guid.NewGuid(),postId);
    }

    private async Task<(Guid Id, CommentResponse Comment)> SeedTestComment(Guid postId)
    {
        var request = CreateUniqueComment(postId);
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var commentId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());
        var comment = await GetCommentById(commentId);
        return (commentId, comment);
    }

    private async Task<CommentResponse> GetCommentById(Guid commentId)
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<CommentResponse>() ?? throw new InvalidOperationException("Failed to deserialize comment response");
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
        var postId = await CreatePost();
        var request = CreateUniqueComment(postId);
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var commentId = await response.Content.ReadFromJsonAsync<Guid>();
        commentId.Should().NotBeEmpty();
        var commentResponse = await GetCommentById(commentId);
        commentResponse.Should().NotBeNull();
        commentResponse.Id.Should().Be(commentId);
        commentResponse.Content.Should().Be(request.Content);
        commentResponse.PostId.Should().Be(request.PostId);
        commentResponse.AuthorId.Should().Be(request.AuthorId);
    }

    [Fact]
    public async Task CreateComment_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Create a post with an authorized client to obtain a valid postId.
        var postId = await CreatePost();
    
        // Now remove the authorization header.
        HttpClient.DefaultRequestHeaders.Authorization = null;
    
        var request = CreateUniqueComment(postId);
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommentById_WithValidId_ShouldReturnComment()
    {
        var postId = await CreatePost();
        var (commentId, seededComment) = await SeedTestComment(postId);
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        comment.Should().BeEquivalentTo(seededComment);
    }

    [Fact]
    public async Task GetCommentById_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidId = Guid.NewGuid();
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{invalidId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComment_WithValidRequest_ShouldReturnOkResult()
    {
        var postId = await CreatePost();
        var (commentId, originalComment) = await SeedTestComment(postId);
        var updateRequest = new UpdateCommentCommand(commentId, "Updated Comment", originalComment.AuthorId);
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{commentId}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedComment = await GetCommentById(commentId);
        updatedComment.Content.Should().Be(updateRequest.Content);
    }

    [Fact]
    public async Task UpdateComment_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidId = Guid.NewGuid();
        var updateRequest = new UpdateCommentCommand(invalidId, "Updated Comment", Guid.NewGuid());
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{invalidId}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WithValidId_ShouldReturnNoContent()
    {
        var postId = await CreatePost();
        var (commentId, seededComment) = await SeedTestComment(postId);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiEndpoint}/{commentId}")
        {
            Content = JsonContent.Create(seededComment.AuthorId)
        };
        var deleteResponse = await HttpClient.SendAsync(request);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{ApiEndpoint}/{invalidId}")
        {
            Content = JsonContent.Create(authorId)
        };
        var response = await HttpClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}