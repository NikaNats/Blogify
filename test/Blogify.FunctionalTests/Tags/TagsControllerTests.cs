using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Tags;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Tags.GetAllTags;
using Blogify.FunctionalTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blogify.FunctionalTests.Tags;

public class TagsControllerTests : BaseFunctionalTest, IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/tags";
    private readonly BlogifyTestSeeder _seeder;

    public TagsControllerTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        var sqlConnectionFactory = factory.Services.GetRequiredService<ISqlConnectionFactory>();
        _seeder = new BlogifyTestSeeder(sqlConnectionFactory);
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

    [Fact]
    public async Task CreateTag_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTagRequest($"New-Tag-{Guid.NewGuid()}");

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var tagId = await response.Content.ReadFromJsonAsync<Guid>();
        tagId.ShouldNotBe(Guid.Empty);

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"{ApiEndpoint}/{tagId}");
    }

    [Fact]
    public async Task CreateTag_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var tagName = $"Duplicate-Tag-{Guid.NewGuid()}";

        await _seeder.SeedTagAsync(tagName);
        var request = new CreateTagRequest(tagName);

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllTags_WhenTagsExist_ShouldReturnOkAndListOfTags()
    {
        // Arrange
        await _seeder.SeedTagAsync("Go");
        await _seeder.SeedTagAsync("CSharp");

        // Act
        var response = await HttpClient.GetAsync(ApiEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<List<AllTagResponse>>();
        tags.ShouldNotBeNull();
        tags.Count.ShouldBeGreaterThanOrEqualTo(2);
        tags.ShouldContain(t => t.Name == "Go");
        tags.ShouldContain(t => t.Name == "CSharp");
    }

    [Fact]
    public async Task GetTagById_WhenTagExists_ShouldReturnOkAndTag()
    {
        // Arrange
        var tagId = await _seeder.SeedTagAsync("Specific-Tag");

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{tagId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tag = await response.Content.ReadFromJsonAsync<AllTagResponse>();
        tag.ShouldNotBeNull();
        tag.Id.ShouldBe(tagId);
        tag.Name.ShouldBe("Specific-Tag");
    }

    [Fact]
    public async Task GetTagById_WhenTagDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTag_WhenTagExists_ShouldReturnNoContent()
    {
        // Arrange
        var tagId = await _seeder.SeedTagAsync();
        var request = new UpdateTagRequest("Updated-Tag-Name");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{tagId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify the update was successful by re-fetching the tag.
        var updatedTag = await HttpClient.GetFromJsonAsync<AllTagResponse>($"{ApiEndpoint}/{tagId}");
        updatedTag.ShouldNotBeNull();
        updatedTag.Name.ShouldBe(request.Name);
    }

    [Fact]
    public async Task UpdateTag_ToExistingName_ShouldReturnConflict()
    {
        // Arrange
        var existingTagName = $"Existing-Tag-{Guid.NewGuid()}";
        await _seeder.SeedTagAsync(existingTagName);
        var tagToUpdateId = await _seeder.SeedTagAsync();
        var request = new UpdateTagRequest(existingTagName);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{tagToUpdateId}", request);
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteTag_WhenTagExists_ShouldReturnNoContent()
    {
        // Arrange
        var tagId = await _seeder.SeedTagAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{tagId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify the tag is gone.
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{tagId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}