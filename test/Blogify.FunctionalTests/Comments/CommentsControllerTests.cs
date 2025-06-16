using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Comments;
using Blogify.Application.Comments;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;
// --- FIX: Import the actual DTOs from the API project ---

namespace Blogify.FunctionalTests.Comments;

public class CommentsControllerTests : BaseFunctionalTest, IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/comments";
    private readonly BlogifyTestSeeder _seeder;

    public CommentsControllerTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        _seeder = new BlogifyTestSeeder(SqlConnectionFactory);
    }

    public async Task InitializeAsync()
    {
        var accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    // Note: The 'CreateComment' related tests are not shown here but would also need
    // to be updated to use the new secure command structure (without AuthorId in the DTO).
    // This fix focuses specifically on the failing Update tests.

    [Fact]
    public async Task GetCommentById_WhenCommentExists_ShouldReturnOkAndComment()
    {
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId);

        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.ShouldNotBeNull();
        comment.Id.ShouldBe(commentId);
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsAuthor_ShouldReturnNoContent()
    {
        // Arrange
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId, AuthenticatedUserId);

        // --- FIX: Use the simplified, correct DTO from the API project ---
        var request = new UpdateCommentRequest("This content was successfully updated by the author.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{commentId}", request);

        // --- FIX: The test now correctly expects 204 NoContent ---
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert (optional but good): verify the change was actually made
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

        // --- FIX: Use the simplified, correct DTO ---
        var request = new UpdateCommentRequest("This update should fail due to authorization.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{otherAuthorsCommentId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    // --- FIX: This test is now obsolete and has been removed. ---
    // The application logic no longer supports a mismatch between route and body IDs,
    // so testing for it is no longer necessary.
    // [Fact]
    // public async Task UpdateComment_WhenRouteIdAndBodyIdMismatch_ShouldReturnBadRequestWithProblemDetails() { ... }

    [Fact]
    public async Task DeleteComment_WhenUserIsAuthor_ShouldReturnNoContent()
    {
        var postId = await _seeder.SeedPostAsync();
        var commentId = await _seeder.SeedCommentAsync(postId, AuthenticatedUserId);

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{commentId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{commentId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsNotAuthor_ShouldReturnConflict()
    {
        var postId = await _seeder.SeedPostAsync();
        var otherAuthorsCommentId = await _seeder.SeedCommentAsync(postId, Guid.NewGuid());

        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{otherAuthorsCommentId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}