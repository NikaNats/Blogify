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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Blogify.FunctionalTests.Posts;

public class PostsControllerTests(FunctionalTestWebAppFactory factory, ITestOutputHelper testOutputHelper)
    : BaseFunctionalTest(factory), IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/posts";


    /// <summary>
    ///     Tags
    /// </summary>
    private const string TagsApiEndpoint = "api/v1/tags";

    private readonly ApplicationDbContext _dbContext =
        factory.Services.CreateScope().ServiceProvider
            .GetRequiredService<ApplicationDbContext>(); // Resolve the DbContext

    private string? _accessToken;

    public async Task InitializeAsync()
    {
        _accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    // public Task DisposeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean up the database after each test
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
                    "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content.")
                .Value,
            PostExcerpt.Create("This is a test post excerpt.").Value,
            Guid.NewGuid() // AuthorId
        );
    }

    private async Task<(Guid Id, PostResponse Post)> SeedTestPost()
    {
        var request = CreateUniquePost();
        testOutputHelper.WriteLine(
            $"Sending request to {ApiEndpoint} with payload: {JsonSerializer.Serialize(request)}");

        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        testOutputHelper.WriteLine($"Response Status: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
            throw new InvalidOperationException($"Request failed: {response.StatusCode} - {errorContent}");
        }

        var postId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());

        // Publish the post
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new InvalidOperationException("Failed to deserialize post response");
    }

    private async Task<Guid> CreateTag(string tagName)
    {
        var createTagCommand = new CreateTagCommand(tagName);
        var response = await HttpClient.PostAsJsonAsync(TagsApiEndpoint, createTagCommand);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<Tag> GetTagById(Guid tagId)
    {
        var response = await HttpClient.GetAsync($"{TagsApiEndpoint}/{tagId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Tag>()
               ?? throw new InvalidOperationException("Failed to deserialize tag response");
    }


    [Fact]
    public async Task CreatePost_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = CreateUniquePost();

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Deserialize the response into a GUID
        var postId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        postId.Should().NotBeEmpty();

        // Optionally, fetch the post by ID to verify its details
        var postResponse = await HttpClient.GetFromJsonAsync<PostResponse>($"{ApiEndpoint}/{postId}");
        postResponse.Should().NotBeNull();
        postResponse.Id.Should().Be(postId);
        postResponse.Title.Should().Be(request.Title.Value);
        postResponse.Content.Should().Be(request.Content.Value);
        postResponse.Excerpt.Should().Be(request.Excerpt.Value);
        postResponse.AuthorId.Should().Be(request.AuthorId);
    }


    [Fact]
    public async Task CreatePost_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var request = CreateUniquePost();

        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllPosts_ShouldReturnNonEmptyList()
    {
        await SeedTestPost();

        var response = await HttpClient.GetAsync(ApiEndpoint);
        var posts = await response.Content.ReadFromJsonAsync<List<PostResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        posts.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public async Task GetPostById_WithValidId_ShouldReturnPost()
    {
        var (postId, seededPost) = await SeedTestPost();

        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{postId}");
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        post.Should().BeEquivalentTo(seededPost);
    }

    [Fact]
    public async Task GetPostById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_WithValidRequest_ShouldReturnOkResult()
    {
        var (postId, _) = await SeedTestPost();
        var updateRequest = new UpdatePostRequest(
            PostTitle.Create("Updated Title").Value,
            PostContent.Create(
                    "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content.")
                .Value,
            PostExcerpt.Create("Updated Excerpt").Value
        );

        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{postId}", updateRequest);
        var updatedPost = await GetPostById(postId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedPost.Should().NotBeNull()
            .And.Match<PostResponse>(p =>
                p.Id == postId &&
                p.Title == updateRequest.Title.Value &&
                p.Content == updateRequest.Content.Value &&
                p.Excerpt == updateRequest.Excerpt.Value);
    }

    [Fact]
    public async Task UpdatePost_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{Guid.NewGuid()}", new UpdatePostRequest(
            PostTitle.Create("Title").Value,
            PostContent.Create(
                    "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content.")
                .Value,
            PostExcerpt.Create("Excerpt").Value
        ));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePost_WithValidId_ShouldReturnNoContent()
    {
        var (postId, _) = await SeedTestPost();

        var deleteResponse = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}");
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{postId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePost_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCommentToPost_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var (postId, seededPost) = await SeedTestPost(); // Seed a post and get its AuthorId

        // Create a valid PostContent object
        var postContentResult = PostContent.Create(
            "This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment."
        );
        postContentResult.IsSuccess.Should().BeTrue(); // Ensure the creation succeeded

        // Use the AuthorId from the seeded post (or mock the authenticated user)
        var authorId = seededPost.AuthorId;

        // Mock the authenticated user's AuthorId
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await GetAccessToken());

        // Create the request
        var request =
            new AddCommentToPostRequest(postContentResult.Value.Value); // Access the Value property of PostContent

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/comments", request);
        var responseContent = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine($"Response Body: {responseContent}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddCommentToPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidPostId = Guid.NewGuid();

        // Create a valid PostContent object
        var postContentResult = PostContent.Create(
            "This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment. This is a valid test comment."
        );
        postContentResult.IsSuccess.Should().BeTrue(); // Ensure the creation succeeded

        // Mock the authenticated user's AuthorId
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await GetAccessToken());

        // Create the request
        var request =
            new AddCommentToPostRequest(postContentResult.Value.Value); // Access the Value property of PostContent

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{invalidPostId}/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCommentToPost_WithEmptyContent_ShouldReturnBadRequest()
    {
        // Arrange
        var (postId, _) = await SeedTestPost();

        // Attempt to create a PostContent with empty content (should fail)
        var postContentResult = PostContent.Create("");
        postContentResult.IsFailure.Should().BeTrue(); // Ensure the creation failed

        // Mock the authenticated user's AuthorId
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await GetAccessToken());

        // Create the request with invalid content
        var request = new AddCommentToPostRequest(""); // Directly pass empty content

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/comments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task AddTagToPost_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName); // Create a tag first
        var request = new AddTagToPostRequest(tagId);

        // Debug: Log the post and tag IDs
        testOutputHelper.WriteLine($"Post ID: {postId}");
        testOutputHelper.WriteLine($"Tag ID: {tagId}");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", request);

        // Debug: Log the response
        testOutputHelper.WriteLine($"AddTagToPost Response Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Fetch the post again to verify the tag association
        var postResponse = await GetPostById(postId);

        // Debug: Log the post and tags
        testOutputHelper.WriteLine($"Post ID: {postResponse.Id}");
        testOutputHelper.WriteLine($"Tags: {JsonSerializer.Serialize(postResponse.Tags)}");

        // Verify the tag is associated with the post
        postResponse.Tags.Should().Contain(t => t.Id == tagId && t.Name == tagName);
    }

    [Fact]
    public async Task AddTagToPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidPostId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var request = new AddTagToPostRequest(tagId);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{invalidPostId}/tags", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddTagToPost_WithInvalidTagId_ShouldReturnBadRequest()
    {
        // Arrange
        var (postId, _) = await SeedTestPost();
        var invalidTagId = Guid.Empty; // Invalid tag ID
        var request = new AddTagToPostRequest(invalidTagId);

        // Debug: Log the request payload
        testOutputHelper.WriteLine($"Request Payload: {JsonSerializer.Serialize(request)}");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", request);

        // Debug: Log the response
        testOutputHelper.WriteLine($"Response Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task RemoveTagFromPost_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var (postId, _) = await SeedTestPost();
        var tagName = "TestTag";
        var tagId = await CreateTag(tagName); // Ensure the tag exists

        // Associate the tag with the post
        var addTagRequest = new AddTagToPostRequest(tagId);
        var addTagResponse = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{postId}/tags", addTagRequest);
        addTagResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Debug: Log the response
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            testOutputHelper.WriteLine($"Error Response: {errorContent}");
        }
    }

    [Fact]
    public async Task RemoveTagFromPost_WithInvalidPostId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidPostId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{invalidPostId}/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveTagFromPost_WithInvalidTagId_ShouldReturnBadRequest()
    {
        // Arrange
        var (postId, _) = await SeedTestPost();
        var invalidTagId = Guid.Empty; // Invalid tag ID

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{postId}/tags/{invalidTagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // [Fact]
    // public async Task CreatePost_WithEmptyTitle_ShouldReturnBadRequest()
    // {
    //     // Arrange
    //     var titleResult = PostTitle.Create(""); // Invalid empty title
    //     if (titleResult.IsSuccess)
    //         throw new InvalidOperationException("Expected title creation to fail, but it succeeded.");
    //
    //     var contentResult =
    //         PostContent.Create(
    //             "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content.");
    //     if (contentResult.IsFailure) throw new InvalidOperationException("Content creation failed unexpectedly.");
    //
    //     var excerptResult = PostExcerpt.Create("Valid excerpt");
    //     if (excerptResult.IsFailure) throw new InvalidOperationException("Excerpt creation failed unexpectedly.");
    //
    //     var request = new CreatePostRequest(
    //         titleResult.Value, // This will throw if titleResult is a failure
    //         contentResult.Value,
    //         excerptResult.Value,
    //         Guid.NewGuid()
    //     );
    //
    //     // Act
    //     var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    // }

    
    [Fact]
    public async Task CreatePost_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Title = "",
            Content = "Valid content that meets the length requirements...",
            Excerpt = "Valid excerpt",
            AuthorId = Guid.NewGuid()
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task CreatePost_WithInvalidAuthorId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreatePostRequest(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(
                    "This is a test post content. This is a test post content. This is a test post content. This is a test post content. This is a test post content.")
                .Value,
            PostExcerpt.Create("Valid excerpt").Value,
            Guid.Empty // Invalid author ID
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}