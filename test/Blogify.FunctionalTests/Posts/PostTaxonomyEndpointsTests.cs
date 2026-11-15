using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Posts;
using Blogify.Application.Categories.GetAllCategoriesWithPostCount;
using Blogify.Application.Posts.GetPostById;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;

namespace Blogify.FunctionalTests.Posts;

public class PostTaxonomyEndpointsTests : BaseFunctionalTest, IAsyncLifetime
{
    private readonly BlogifyTestSeeder _seeder;

    public PostTaxonomyEndpointsTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        _seeder = new BlogifyTestSeeder(SqlConnectionFactory);
    }

    public async ValueTask InitializeAsync()
    {
        var token = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async ValueTask DisposeAsync() => await ResetCacheAsync();

    [Fact]
    public async Task AssignAndRemoveCategory_ShouldUpdatePostAndPostCounts()
    {
        var postId = await _seeder.SeedPostAsync(AuthenticatedUserId);
        var categoryId = await _seeder.SeedCategoryAsync($"Cat-{Guid.NewGuid():N}");

        var assign = await HttpClient.PostAsJsonAsync($"api/v1/posts/{postId}/categories",
            new AssignCategoriesToPostRequest([categoryId]));
        assign.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var post = await HttpClient.GetFromJsonAsync<PostResponse>($"api/v1/posts/{postId}");
        post!.Categories.ShouldContain(c => c.Id == categoryId);

        var counts = await HttpClient.GetFromJsonAsync<List<CategoryWithPostCountResponse>>(
            "api/v1/categories/with-post-count");
        counts!.Single(c => c.Id == categoryId).PostCount.ShouldBe(1);

        var remove = await HttpClient.DeleteAsync($"api/v1/posts/{postId}/categories/{categoryId}");
        remove.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        post = await HttpClient.GetFromJsonAsync<PostResponse>($"api/v1/posts/{postId}");
        post!.Categories.ShouldNotContain(c => c.Id == categoryId);
    }

    [Fact]
    public async Task DeleteCategory_ShouldRemoveItFromPosts()
    {
        var postId = await _seeder.SeedPostAsync(AuthenticatedUserId);
        var categoryId = await _seeder.SeedCategoryAsync($"Cat-{Guid.NewGuid():N}");
        (await HttpClient.PostAsJsonAsync($"api/v1/posts/{postId}/categories",
            new AssignCategoriesToPostRequest([categoryId]))).EnsureSuccessStatusCode();

        (await HttpClient.DeleteAsync($"api/v1/categories/{categoryId}")).StatusCode
            .ShouldBe(HttpStatusCode.NoContent);

        var post = await HttpClient.GetFromJsonAsync<PostResponse>($"api/v1/posts/{postId}");
        post!.Categories.ShouldNotContain(c => c.Id == categoryId);
    }

    [Fact]
    public async Task DeleteTag_ShouldRemoveItFromPosts()
    {
        var postId = await _seeder.SeedPostAsync(AuthenticatedUserId);
        var tagId = await _seeder.SeedTagAsync($"tag-{Guid.NewGuid():N}"[..20]);
        (await HttpClient.PostAsJsonAsync($"api/v1/posts/{postId}/tags", new AddTagToPostRequest(tagId)))
            .EnsureSuccessStatusCode();

        (await HttpClient.DeleteAsync($"api/v1/tags/{tagId}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var post = await HttpClient.GetFromJsonAsync<PostResponse>($"api/v1/posts/{postId}");
        post!.Tags.ShouldNotContain(t => t.Id == tagId);
    }
}
