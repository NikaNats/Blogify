using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Comments;
using Blogify.Application.Comments;
using Blogify.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Blogify.FunctionalTests.Comments;

public class CommentsControllerTests : BaseFunctionalTest, IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/comments";
    private readonly BlogifyTestSeeder _seeder;

    public CommentsControllerTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        // The base constructor provides the SqlConnectionFactory
        _seeder = new BlogifyTestSeeder(SqlConnectionFactory);
    }

    public async Task InitializeAsync()
    {
        // This helper gets an access token and sets the AuthenticatedUserId property
        var accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
        // Testcontainers handle resource cleanup
    }

    [Fact]
    public async Task CreateComment_WithValidData_ShouldReturnCreatedAndLocationHeader()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var request = new CreateCommentRequest("This is a newly created comment.", AuthenticatedUserId, postId);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdCommentId = await response.Content.ReadFromJsonAsync<Guid>();
        createdCommentId.ShouldNotBe(Guid.Empty);

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"{ApiEndpoint}/{createdCommentId}");
    }

    [Fact]
    public async Task CreateComment_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var request = new CreateCommentRequest("This comment should be rejected.", Guid.NewGuid(), postId);
        HttpClient.DefaultRequestHeaders.Authorization = null; // Remove authentication for this test

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommentById_WhenCommentExists_ShouldReturnOkAndComment()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId);

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.ShouldNotBeNull();
        comment.Id.ShouldBe(commentId);
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsAuthor_ShouldReturnOk()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId, AuthenticatedUserId);
        var request = new UpdateCommentRequest(commentId, "This content was successfully updated by the author.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{commentId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedComment = await HttpClient.GetFromJsonAsync<CommentResponse>($"{ApiEndpoint}/{commentId}");
        updatedComment.ShouldNotBeNull();
        updatedComment.Content.ShouldBe(request.Content);
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsNotAuthor_ShouldReturnConflict()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var otherAuthorsCommentId = await _seeder.SeedCommentAsync(postId, Guid.NewGuid());
        var request = new UpdateCommentRequest(otherAuthorsCommentId, "This update should fail due to authorization.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{otherAuthorsCommentId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateComment_WhenRouteIdAndBodyIdMismatch_ShouldReturnBadRequestWithProblemDetails()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var actualCommentId = await _seeder.SeedCommentAsync(postId, AuthenticatedUserId);
        var mismatchedCommentIdInBody = Guid.NewGuid();
        var request = new UpdateCommentRequest(mismatchedCommentIdInBody, "Mismatched IDs update attempt.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{actualCommentId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        // This now correctly asserts against the title produced by our unified ExceptionHandlingMiddleware
        problemDetails.Title.ShouldBe("Validation");
        problemDetails.Extensions.ShouldContainKey("errors");
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsAuthor_ShouldReturnNoContent()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId, AuthenticatedUserId);

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{commentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsNotAuthor_ShouldReturnConflict()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var otherAuthorsCommentId = await _seeder.SeedCommentAsync(postId, Guid.NewGuid());

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{otherAuthorsCommentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    // API contract DTOs for clarity
    private record CreateCommentRequest(string Content, Guid AuthorId, Guid PostId);
}