namespace Blogify.Domain.UnitTests.Category
{
    public class CategoryTests
    {
        // Test for successful creation of a Category
        [Fact]
        public void Create_ValidInputs_ReturnsSuccessResultWithCategory()
        {
            // Arrange
            const string name = "Technology";
            const string description = "All about technology.";

            // Act
            var result = Categories.Category.Create(name, description);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(name, result.Value.Name);
            Assert.Equal(description, result.Value.Description);
            Assert.NotEqual(default, result.Value.CreatedAt);
        }

        // Test for empty name
        [Fact]
        public void Create_EmptyName_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var name = string.Empty;
            const string description = "All about technology.";

            // Act
            var result = Categories.Category.Create(name, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Name", result.Error.Code);
            Assert.Equal("Name cannot be empty.", result.Error.Description);
        }

        // Test for null name
        [Fact]
        public void Create_NullName_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            string name = null;
            const string description = "All about technology.";

            // Act
            var result = Categories.Category.Create(name, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Name", result.Error.Code);
            Assert.Equal("Name cannot be empty.", result.Error.Description);
        }

        // Test for empty description
        [Fact]
        public void Create_EmptyDescription_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            const string name = "Technology";
            var description = string.Empty;

            // Act
            var result = Categories.Category.Create(name, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Description", result.Error.Code);
            Assert.Equal("Description cannot be empty.", result.Error.Description);
        }

        // Test for null description
        [Fact]
        public void Create_NullDescription_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            const string name = "Technology";
            string description = null;

            // Act
            var result = Categories.Category.Create(name, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Description", result.Error.Code);
            Assert.Equal("Description cannot be empty.", result.Error.Description);
        }

        // Test for successful update of a Category
        [Fact]
        public void Update_ValidInputs_ReturnsSuccessResult()
        {
            // Arrange
            var category = Categories.Category.Create("Technology", "All about technology.").Value;
            var newName = "Tech";
            var newDescription = "Latest in tech.";

            // Act
            var result = category.Update(newName, newDescription);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newName, category.Name);
            Assert.Equal(newDescription, category.Description);
            Assert.NotNull(category.UpdatedAt);
        }

        // Test for empty name during update
        [Fact]
        public void Update_EmptyName_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var category = Categories.Category.Create("Technology", "All about technology.").Value;
            var newName = string.Empty;
            var newDescription = "Latest in tech.";

            // Act
            var result = category.Update(newName, newDescription);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Name", result.Error.Code);
            Assert.Equal("Name cannot be empty.", result.Error.Description);
        }

        // Test for null name during update
        [Fact]
        public void Update_NullName_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var category = Categories.Category.Create("Technology", "All about technology.").Value;
            string newName = null;
            var newDescription = "Latest in tech.";

            // Act
            var result = category.Update(newName, newDescription);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Name", result.Error.Code);
            Assert.Equal("Name cannot be empty.", result.Error.Description);
        }

        // Test for empty description during update
        [Fact]
        public void Update_EmptyDescription_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var category = Categories.Category.Create("Technology", "All about technology.").Value;
            const string newName = "Tech";
            var newDescription = string.Empty;

            // Act
            var result = category.Update(newName, newDescription);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Description", result.Error.Code);
            Assert.Equal("Description cannot be empty.", result.Error.Description);
        }

        // Test for null description during update
        [Fact]
        public void Update_NullDescription_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var category = Categories.Category.Create("Technology", "All about technology.").Value;
            const string newName = "Tech";
            string newDescription = null;

            // Act
            var result = category.Update(newName, newDescription);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.Description", result.Error.Code);
            Assert.Equal("Description cannot be empty.", result.Error.Description);
        }
    }
}