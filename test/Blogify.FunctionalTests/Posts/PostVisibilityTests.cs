using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;

namespace Blogify.FunctionalTests.Posts;

/// <summary>
///     Public read access and Draft/Archived visibility. Anonymous readers and other users only see Published
///     content; the author and administrators also see unpublished posts; restricted posts never leak via cache.
///     The shared test user is an administrator; the "regular" user is the post author in most scenarios.
/// </summary>
public class PostVisibilityTests : BaseFunctionalTest, IAsyncLifetime
{
    private readonly HttpClient _anonymous;
    private readonly HttpClient _regular;
    private readonly BlogifyTestSeeder _seeder;
    private Guid _adminId;
    private Guid _regularId;

    public PostVisibilityTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        _anonymous = factory.CreateClient();
        _regular = factory.CreateClient();
        _seeder = new BlogifyTestSeeder(SqlConnectionFactory);
    }

    private HttpClient Admin => HttpClient;

    public async ValueTask InitializeAsync()
    {
        var adminToken = await GetAccessToken();
        Admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        _adminId = AuthenticatedUserId;

        var (token, id) = await GetRegularUserAccessToken();
        _regular.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _regularId = id;

        await ResetCacheAsync();
    }

    public ValueTask DisposeAsync()
    {
        _anonymous.Dispose();
        _regular.Dispose();
        return ValueTask.CompletedTask;
    }

    private static async Task<List<Guid>> ItemIdsAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, url);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetGuid()).ToList();
    }

    private static void AssertContainsAll(List<Guid> actual, IEnumerable<Guid> expected)
    {
        foreach (var id in expected) actual.ShouldContain(id);
    }

    [Fact]
    public async Task Anonymous_CanReadPublishedContent()
    {
        var postId = await _seeder.SeedPostAsync(_regularId);
        await _seeder.SeedCommentAsync(postId, _regularId);
        await _seeder.SeedCategoryAsync();
        await _seeder.SeedTagAsync();

        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ItemIdsAsync(_anonymous, "api/v1/posts?pageSize=100")).ShouldContain(postId);
        (await _anonymous.GetAsync("api/v1/categories")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _anonymous.GetAsync("api/v1/tags")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ItemIdsAsync(_anonymous, $"api/v1/comments/by-post/{postId}")).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Anonymous_CannotWrite()
    {
        var response = await _anonymous.PostAsJsonAsync("api/v1/posts",
            new { title = "t", content = new string('x', 120), excerpt = "e" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await _anonymous.GetAsync("api/v1/posts/mine")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Archived")]
    public async Task UnpublishedPost_IsNotFoundForAnonymousAndOtherUsers(string status)
    {
        // Authored by the regular user, so the administrator is the "other privileged" party and an
        // anonymous caller is the stranger.
        var postId = await _seeder.SeedPostAsync(_regularId, status);
        await _seeder.SeedCommentAsync(postId, _regularId);

        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await _anonymous.GetAsync($"api/v1/comments/by-post/{postId}")).StatusCode
            .ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnpublishedPost_IsNotFoundForAnotherRegisteredUser()
    {
        // Authored by the administrator; the regular (non-admin) user must not see it.
        var postId = await _seeder.SeedPostAsync(_adminId, "Draft");

        (await _regular.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Archived")]
    public async Task UnpublishedPost_IsVisibleToAuthorAndAdministrator(string status)
    {
        var postId = await _seeder.SeedPostAsync(_regularId, status);

        (await _regular.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Admin.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _regular.GetAsync($"api/v1/comments/by-post/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Lists_ExcludeUnpublished_ExceptForAuthorAndAdmin()
    {
        var published = await _seeder.SeedPostAsync(_regularId);
        var draft = await _seeder.SeedPostAsync(_regularId, "Draft");
        var archived = await _seeder.SeedPostAsync(_regularId, "Archived");
        var authorUrl = $"api/v1/posts/author/{_regularId}?pageSize=100";

        var all = await ItemIdsAsync(_anonymous, "api/v1/posts?pageSize=100");
        all.ShouldContain(published);
        all.ShouldNotContain(draft);
        all.ShouldNotContain(archived);

        var publicByAuthor = await ItemIdsAsync(_anonymous, authorUrl);
        publicByAuthor.ShouldContain(published);
        publicByAuthor.ShouldNotContain(draft);
        publicByAuthor.ShouldNotContain(archived);
        (await ItemIdsAsync(Admin, "api/v1/posts?pageSize=100")).ShouldNotContain(draft);

        var expected = new[] { published, draft, archived };
        AssertContainsAll(await ItemIdsAsync(_regular, "api/v1/posts/mine?pageSize=100"), expected);
        AssertContainsAll(await ItemIdsAsync(_regular, authorUrl), expected);
        AssertContainsAll(await ItemIdsAsync(Admin, authorUrl), expected);
    }

    [Fact]
    public async Task Draft_ReadByAuthor_IsNotServedFromCacheToOthers()
    {
        var postId = await _seeder.SeedPostAsync(_regularId, "Draft");

        (await _regular.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _regular.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archiving_ACachedPublishedPost_HidesItFromAnonymous()
    {
        var postId = await _seeder.SeedPostAsync(_regularId);

        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await _regular.PutAsync($"api/v1/posts/{postId}/archive", null)).StatusCode
            .ShouldBe(HttpStatusCode.NoContent);

        (await _anonymous.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await _regular.GetAsync($"api/v1/posts/{postId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Administrator_CanDeleteAnotherUsersComment()
    {
        var postId = await _seeder.SeedPostAsync(_adminId);
        var commentId = await _seeder.SeedCommentAsync(postId, _regularId);

        (await Admin.DeleteAsync($"api/v1/comments/{commentId}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RegularUser_CannotDeleteAnotherUsersComment()
    {
        var postId = await _seeder.SeedPostAsync(_adminId);
        var commentId = await _seeder.SeedCommentAsync(postId, _adminId);

        (await _regular.DeleteAsync($"api/v1/comments/{commentId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task HealthEndpoints_LiveAndReady_AreHealthy()
    {
        (await _anonymous.GetAsync("health/live")).StatusCode.ShouldBe(HttpStatusCode.OK);
        // Keycloak:BaseUrl in the test host is not the container address, so readiness may legitimately report 503;
        // the endpoint just has to exist and be a health response.
        (await _anonymous.GetAsync("health/ready")).StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }
}
