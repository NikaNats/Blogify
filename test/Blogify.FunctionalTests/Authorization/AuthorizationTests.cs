using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Categories;
using Blogify.Api.Controllers.Posts;
using Blogify.Api.Controllers.Tags;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;

namespace Blogify.FunctionalTests.Authorization;

/// <summary>
///     Verifies that a regular (non-administrator) Registered user is rejected with 403 on admin-only and
///     owner-only operations. The shared test user is an administrator, so these tests use a separate user.
/// </summary>
public class AuthorizationTests : BaseFunctionalTest, IAsyncLifetime
{
    private readonly HttpClient _regularClient;
    private readonly BlogifyTestSeeder _seeder;
    private Guid _adminUserId;

    public AuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        _regularClient = factory.CreateClient();
        _seeder = new BlogifyTestSeeder(SqlConnectionFactory);
    }

    public async ValueTask InitializeAsync()
    {
        // The admin owns the seeded posts, the regular user is the one attempting the writes.
        await GetAccessToken();
        _adminUserId = AuthenticatedUserId;

        var (token, _) = await GetRegularUserAccessToken();
        _regularClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public ValueTask DisposeAsync()
    {
        _regularClient.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateCategory_AsRegularUser_ShouldReturnForbidden()
    {
        var response = await _regularClient.PostAsJsonAsync("api/v1/categories",
            new CreateCategoryRequest($"Cat {Guid.NewGuid()}", "Description"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_AsRegularUser_ShouldReturnForbidden()
    {
        var id = await _seeder.SeedCategoryAsync();

        var response = await _regularClient.PutAsJsonAsync($"api/v1/categories/{id}",
            new UpdateCategoryRequest($"Renamed {Guid.NewGuid()}", "Description"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCategory_AsRegularUser_ShouldReturnForbidden()
    {
        var id = await _seeder.SeedCategoryAsync();

        var response = await _regularClient.DeleteAsync($"api/v1/categories/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTag_AsRegularUser_ShouldReturnForbidden()
    {
        var response = await _regularClient.PostAsJsonAsync("api/v1/tags",
            new CreateTagRequest($"tag-{Guid.NewGuid():N}"[..20]));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateTag_AsRegularUser_ShouldReturnForbidden()
    {
        var id = await _seeder.SeedTagAsync();

        var response = await _regularClient.PutAsJsonAsync($"api/v1/tags/{id}",
            new UpdateTagRequest($"tag-{Guid.NewGuid():N}"[..20]));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTag_AsRegularUser_ShouldReturnForbidden()
    {
        var id = await _seeder.SeedTagAsync();

        var response = await _regularClient.DeleteAsync($"api/v1/tags/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePost_OwnedByAnotherUser_ShouldReturnForbidden()
    {
        var postId = await _seeder.SeedPostAsync(_adminUserId);

        var response = await _regularClient.PutAsJsonAsync($"api/v1/posts/{postId}",
            new UpdatePostRequest("Hijacked title",
                new string('x', 120),
                "Hijacked excerpt"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePost_OwnedByAnotherUser_ShouldReturnForbidden()
    {
        var postId = await _seeder.SeedPostAsync(_adminUserId);

        var response = await _regularClient.DeleteAsync($"api/v1/posts/{postId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishPost_OwnedByAnotherUser_ShouldReturnForbidden()
    {
        var postId = await _seeder.SeedPostAsync(_adminUserId, "Draft");

        var response = await _regularClient.PutAsync($"api/v1/posts/{postId}/publish", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ArchivePost_OwnedByAnotherUser_ShouldReturnForbidden()
    {
        var postId = await _seeder.SeedPostAsync(_adminUserId);

        var response = await _regularClient.PutAsync($"api/v1/posts/{postId}/archive", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
