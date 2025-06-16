using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Categories;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.FunctionalTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blogify.FunctionalTests.Categories;

public class CategoriesControllerTests : BaseFunctionalTest, IAsyncLifetime
{
    private const string ApiEndpoint = "api/v1/categories";
    private readonly BlogifyTestSeeder _seeder;

    public CategoriesControllerTests(FunctionalTestWebAppFactory factory) : base(factory)
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
    public async Task CreateCategory_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCategoryRequest($"New Category-{Guid.NewGuid()}", "A valid description.");

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var categoryId = await response.Content.ReadFromJsonAsync<Guid>();
        categoryId.ShouldNotBe(Guid.Empty);

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldContain($"{ApiEndpoint}/{categoryId}");
    }

    [Fact]
    public async Task GetAllCategories_WhenCategoriesExist_ShouldReturnOkAndListOfCategories()
    {
        // Arrange
        await _seeder.SeedCategoryAsync("Tech");
        await _seeder.SeedCategoryAsync("Health");

        // Act
        var response = await HttpClient.GetAsync(ApiEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<AllCategoryResponse>>();
        categories.ShouldNotBeNull();
        categories.Count.ShouldBeGreaterThanOrEqualTo(2);
        categories.ShouldContain(c => c.Name == "Tech");
        categories.ShouldContain(c => c.Name == "Health");
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryExists_ShouldReturnOkAndCategory()
    {
        // Arrange
        var categoryId = await _seeder.SeedCategoryAsync("Specific Category");

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{categoryId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<AllCategoryResponse>();
        category.ShouldNotBeNull();
        category.Id.ShouldBe(categoryId);
        category.Name.ShouldBe("Specific Category");
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryExists_ShouldReturnOk()
    {
        // Arrange
        var categoryId = await _seeder.SeedCategoryAsync();
        var request = new UpdateCategoryRequest("Updated Name", "Updated description.");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{categoryId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedCategory = await HttpClient.GetFromJsonAsync<AllCategoryResponse>($"{ApiEndpoint}/{categoryId}");
        updatedCategory.ShouldNotBeNull();
        updatedCategory.Name.ShouldBe(request.Name);
        updatedCategory.Description.ShouldBe(request.Description);
    }

    [Fact]
    public async Task DeleteCategory_WhenCategoryExists_ShouldReturnNoContent()
    {
        // Arrange
        var categoryId = await _seeder.SeedCategoryAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{categoryId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{categoryId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}