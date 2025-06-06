using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blogify.Api.Controllers.Categories;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace Blogify.FunctionalTests.Categories;

public class CategoriesControllerTests(FunctionalTestWebAppFactory factory, ITestOutputHelper testOutputHelper)
    : BaseFunctionalTest(factory), IAsyncLifetime
{
    private string? _accessToken;
    private const string ApiEndpoint = "api/v1/categories";

    public async Task InitializeAsync()
    {
        _accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateCategoryRequest CreateUniqueCategory() => 
        new($"Test Category {Guid.NewGuid()}", "Test Description");

    private async Task<(Guid Id, CategoryResponse Category)> SeedTestCategory()
    {
        var request = CreateUniqueCategory();
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine($"Created category response: {jsonResponse}");

        var categoryId = JsonSerializer.Deserialize<Guid>(jsonResponse);
        var category = await GetCategoryById(categoryId);

        return (categoryId, category);
    }

    private async Task<CategoryResponse> GetCategoryById(Guid categoryId)
    {
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{categoryId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        category.ShouldNotBeNull();
        return category;
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = CreateUniqueCategory();

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var categoryId = JsonSerializer.Deserialize<Guid>(jsonResponse);
        var createdCategory = await GetCategoryById(categoryId);

        createdCategory.ShouldNotBeNull();
        createdCategory.Id.ShouldBe(categoryId);
        createdCategory.Name.ShouldBe(request.Name);
        createdCategory.Description.ShouldBe(request.Description);
    }

    [Fact]
    public async Task CreateCategory_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var request = CreateUniqueCategory();

        // Act
        var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnNonEmptyList()
    {
        // Arrange
        await SeedTestCategory();

        // Act
        var response = await HttpClient.GetAsync(ApiEndpoint);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        categories.ShouldNotBeNull();
        categories.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetCategoryById_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var (categoryId, seededCategory) = await SeedTestCategory();

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{categoryId}");
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        category.ShouldNotBeNull();
        category.Id.ShouldBe(seededCategory.Id);
        category.Name.ShouldBe(seededCategory.Name);
        category.Description.ShouldBe(seededCategory.Description);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"{ApiEndpoint}/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var (categoryId, _) = await SeedTestCategory();
        var updateRequest = new UpdateCategoryRequest("Updated Category", "Updated Description");

        // Act
        var updateResponse = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{categoryId}", updateRequest);
        var updatedCategory = await GetCategoryById(categoryId);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        updatedCategory.ShouldNotBeNull();
        updatedCategory.Id.ShouldBe(categoryId);
        updatedCategory.Name.ShouldBe(updateRequest.Name);
        updatedCategory.Description.ShouldBe(updateRequest.Description);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = new UpdateCategoryRequest("Updated Category", "Updated Description");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var (categoryId, _) = await SeedTestCategory();

        // Act
        var deleteResponse = await HttpClient.DeleteAsync($"{ApiEndpoint}/{categoryId}");
        var getResponse = await HttpClient.GetAsync($"{ApiEndpoint}/{categoryId}");

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}