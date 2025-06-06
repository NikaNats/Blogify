using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blogify.Api.Controllers.Posts;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Tags.CreateTag;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
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
        var posts = await _dbContext.Set<Post>().ToListAsync();
        _dbContext.Set<Post>().RemoveRange(posts);

        var tags = await _dbContext.Set<Tag>().ToListAsync();
        _dbContext.Set<Tag>().RemoveRange(tags);

        await _dbContext.SaveChangesAsync();
    }

    private static CreatePostRequest CreateUniquePost()
    {
        return new CreatePostRequest(
            PostTitle.Create($"Test Post {Guid.NewGuid()}").Value,
            PostContent.Create(
                "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content."
            ).Value,
            PostExcerpt.Create("This is a test post excerpt.").Value,
            Guid.NewGuid() // AuthorId
        );
    }

    private async Task<(Guid Id, PostResponse Post)> SeedTestPost()
    {
        var request = CreateUniquePost();
        testOutputHelper.WriteLine($"Sending request to {ApiEndpoint} with payload: {JsonSerializer.Serialize(request)}");

        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        testOutputHelper.WriteLine($"Response Status: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
            throw new InvalidOperationException($"Request failed: {response.StatusCode} - {errorContent}");
        }

        var postId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());

        var publishResponse = await HttpClient.PutAsync($"{ApiEndpoint}/{postId}/publish", null);
        testOutputHelper.WriteLine($"Publish Post Response Status: {publishResponse.StatusCode}");

        if (!publishResponse.IsSuccessStatusCode)
        {
            var errorContent = await publishResponse.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
            throw new InvalidOperationException($"Request failed: {publishResponse.StatusCode} - {errorContent}");
        }

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

    private async Task<Tag> GetTagById(Guid tagId)
    {
        var response = await HttpClient.GetAsync($"{TagsApiEndpoint}/{tagId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tag = await response.Content.ReadFromJsonAsync<Tag>();
        tag.ShouldNotBeNull();
        return tag;
    }

    [Fact]
    public async Task CreatePost_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = CreateUniquePost();

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Deserialize the response into a GUID
        var postId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        postId.ShouldNotBe(Guid.Empty);

        // Optionally, fetch the post by ID to verify its details
        var postResponse = await HttpClient.GetFromJsonAsync<PostResponse>($"{ApiEndpoint}/{postId}");
        postResponse.ShouldNotBeNull();
        postResponse.Id.ShouldBe(postId);
        postResponse.Title.ShouldBe(request.Title.Value);
        postResponse.Content.ShouldBe(request.Content.Value);
        postResponse.Excerpt.ShouldBe(request.Excerpt.Value);
        postResponse.AuthorId.ShouldBe(request.AuthorId);
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
    public async Task GetAllPosts_ShouldReturnNonEmptyList()
    {
        await SeedTestPost();

        var response = await HttpClient.GetAsync(ApiEndpoint);
        var posts = await response.Content.ReadFromJsonAsync<List<PostResponse>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        posts.ShouldNotBeNull();
        posts.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetPostById_WithValidId_ShouldReturnPost()
    {
        var (postId, seededPost) = await SeedTestPost();

        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{postId}");
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        post.ShouldNotBeNull();
        post.Id.ShouldBe(seededPost.Id);
        post.Title.ShouldBe(seededPost.Title);
        post.Content.ShouldBe(seededPost.Content);
        post.Excerpt.ShouldBe(seededPost.Excerpt);
        post.AuthorId.ShouldBe(seededPost.AuthorId);
    }

    [Fact]
    public async Task GetPostById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_WithValidRequest_ShouldReturnOkResult()
    {
        var (postId, _) = await SeedTestPost();
        var updateRequest = new UpdatePostRequest(
            PostTitle.Create("Updated Title").Value,
            PostContent.Create(
                "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content."
            ).Value,
            PostExcerpt.Create("Updated Excerpt").Value
        );

        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{postId}", updateRequest);
        var updatedPost = await GetPostById(postId);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updatedPost.ShouldNotBeNull();
        updatedPost.Id.ShouldBe(postId);
        updatedPost.Title.ShouldBe(updateRequest.Title.Value);
        updatedPost.Content.ShouldBe(updateRequest.Content.Value);
        updatedPost.Excerpt.ShouldBe(updateRequest.Excerpt.Value);
    }

    [Fact]
    public async Task UpdatePost_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{Guid.NewGuid()}", new UpdatePostRequest(
            PostTitle.Create("Title").Value,
            PostContent.Create(
                "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content."
            ).Value,
            PostExcerpt.Create("Excerpt").Value
        ));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
    public async Task DeletePost_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCommentToPost_WithValidRequest_ShouldReturnOk()
    {
        var (postId, seededPost) = await SeedTestPost();
        var postContentResult = PostContent.Create(
            "This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment."
        );
        postContentResult.IsSuccess.ShouldBeTrue();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
        var request = new AddCommentToPostRequest(postContentResult.Value.Value);

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/comments", request);
        var responseContent = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine($"Response Body: {responseContent}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddCommentToPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        var invalidPostId = Guid.NewGuid();
        var postContentResult = PostContent.Create(
            "This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment."
        );
        postContentResult.IsSuccess.ShouldBeTrue();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
        var request = new AddCommentToPostRequest(postContentResult.Value.Value);

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{invalidPostId}/comments", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCommentToPost_WithEmptyContent_ShouldReturnBadRequest()
    {
        var (postId, _) = await SeedTestPost();
        var postContentResult = PostContent.Create("");
        postContentResult.IsFailure.ShouldBeTrue();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
        var request = new AddCommentToPostRequest("");

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/comments", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddTagToPost_WithValidRequest_ShouldReturnOk()
    {
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName);
        var request = new AddTagToPostRequest(tagId);

        testOutputHelper.WriteLine($"Post ID: {postId}");
        testOutputHelper.WriteLine($"Tag ID: {tagId}");

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", request);

        testOutputHelper.WriteLine($"AddTagToPost Response Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var postResponse = await GetPostById(postId);
        testOutputHelper.WriteLine($"Post ID: {postResponse.Id}");
        testOutputHelper.WriteLine($"Tags: {JsonSerializer.Serialize(postResponse.Tags)}");

        postResponse.Tags.ShouldContain(t => t.Id == tagId && t.Name == tagName);
    }

    [Fact]
    public async Task AddTagToPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        var invalidPostId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var request = new AddTagToPostRequest(tagId);

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{invalidPostId}/tags", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddTagToPost_WithInvalidTagId_ShouldReturnBadRequest()
    {
        var (postId, _) = await SeedTestPost();
        var invalidTagId = Guid.Empty;
        var request = new AddTagToPostRequest(invalidTagId);

        testOutputHelper.WriteLine($"Request Payload: {JsonSerializer.Serialize(request)}");

        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", request);

        testOutputHelper.WriteLine($"Response Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveTagFromPost_WithValidRequest_ShouldReturnOk()
    {
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName);

        var addTagRequest = new AddTagToPostRequest(tagId);
        var addTagResponse = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", addTagRequest);
        addTagResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}/tags/{tagId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }
    }

    [Fact]
    public async Task RemoveTagFromPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        var invalidPostId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{invalidPostId}/tags/{tagId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveTagFromPost_WithInvalidTagId_ShouldReturnBadRequest()
    {
        var (postId, _) = await SeedTestPost();
        var invalidTagId = Guid.Empty;

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}/tags/{invalidTagId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePost_WithEmptyTitle_ShouldReturnBadRequest()
    {
        var request = new
        {
            Title = "",
            Content = "Valid content that meets the length requirements...",
            Excerpt = "Valid excerpt",
            AuthorId = Guid.NewGuid()
        };

        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePost_WithInvalidAuthorId_ShouldReturnBadRequest()
    {
        var request = new CreatePostRequest(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(
                "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content."
            ).Value,
            PostExcerpt.Create("Valid excerpt").Value,
            Guid.Empty // Invalid author ID
        );

        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}