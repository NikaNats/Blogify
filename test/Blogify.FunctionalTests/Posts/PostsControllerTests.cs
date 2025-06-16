using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Posts;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Tags.CreateTag;
using Blogify.FunctionalTests.Infrastructure;
using Blogify.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Blogify.FunctionalTests.Posts;

public class PostsControllerTests(FunctionalTestWebAppFactory factory, ITestOutputHelper testOutputHelper)
    : BaseFunctionalTest(factory), IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/posts";
    private const string TagsApiEndpoint = "api/v1/tags";

    private readonly ApplicationDbContext _dbContext =
        factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

    private string? _accessToken;
    private Guid _authenticatedUserId;

    public async Task InitializeAsync()
    {
        _accessToken = await GetAccessToken();
        _authenticatedUserId = AuthenticatedUserId;
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public Task DisposeAsync()
    {
        return _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE posts, tags, comments, categories RESTART IDENTITY");
    }

    private static CreatePostRequest CreateUniquePost()
    {
        return new CreatePostRequest(
            $"Test Post {Guid.NewGuid()}",
            "This is a test post content that is very long to satisfy any potential validation rules. It repeats to ensure it meets the minimum length requirement.",
            "This is a test post excerpt."
        );
    }

    private static UpdatePostRequest CreateUniqueUpdatePost()
    {
        return new UpdatePostRequest(
            "Updated Title",
            "This is the updated test post content, and it is also very long to satisfy any potential validation rules. It repeats to ensure it meets the minimum length requirement.",
            "This is an updated test post excerpt."
        );
    }

    private async Task<(Guid Id, PostResponse Post)> SeedTestPost()
    {
        var request = CreateUniquePost();
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.EnsureSuccessStatusCode();

        var postId = await response.Content.ReadFromJsonAsync<Guid>();
        (await HttpClient.PutAsync($"{ApiEndpoint}/{postId}/publish", null)).EnsureSuccessStatusCode();
        var post = await GetPostById(postId);
        return (postId, post);
    }

    private async Task<PostResponse> GetPostById(Guid postId)
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{postId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.ShouldNotBeNull();
        return post;
    }

    private async Task<Guid> CreateTag(string tagName)
    {
        var createTagCommand = new CreateTagCommand(tagName);
        var response = await HttpClient.PostAsJsonAsync(TagsApiEndpoint, createTagCommand);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task CreatePost_WithValidRequest_ShouldReturnCreatedWithCorrectAuthorId()
    {
        var request = CreateUniquePost();
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        var postId = await response.Content.ReadFromJsonAsync<Guid>();
        var postResponse = await GetPostById(postId);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        postResponse.Title.ShouldBe(request.Title);
        postResponse.Content.ShouldBe(request.Content);
        postResponse.AuthorId.ShouldBe(_authenticatedUserId);
    }

    [Fact]
    public async Task CreatePost_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var request = CreateUniquePost();
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePost_WithValidRequest_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();
        var updateRequest = CreateUniqueUpdatePost();

        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{postId}", updateRequest);
        // --- FIX: The expected status code is now NoContent ---
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updatedPost = await GetPostById(postId);
        updatedPost.Title.ShouldBe(updateRequest.Title);
        updatedPost.Content.ShouldBe(updateRequest.Content);
        updatedPost.Excerpt.ShouldBe(updateRequest.Excerpt);
    }

    [Fact]
    public async Task DeletePost_WithValidId_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();
        var deleteResponse = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}");
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{postId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCommentToPost_WithValidRequest_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();
        var request = new AddCommentToPostRequest("This is a valid test comment.");
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/comments", request);
        // --- FIX: The expected status code is now NoContent ---
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddTagToPost_WithValidRequest_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName);
        var request = new AddTagToPostRequest(tagId);

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", request);
        // --- FIX: The expected status code is now NoContent ---
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var postResponse = await GetPostById(postId);
        postResponse.Tags.ShouldContain(t => t.Id == tagId);
    }

    [Fact]
    public async Task RemoveTagFromPost_WithValidRequest_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName);
        var addTagRequest = new AddTagToPostRequest(tagId);
        (await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", addTagRequest)).EnsureSuccessStatusCode();

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}/tags/{tagId}");

        // This test was missing an assertion. We now assert for NoContent.
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var postResponse = await GetPostById(postId);
        postResponse.Tags.ShouldNotContain(t => t.Id == tagId);
    }
}