using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public static class CategoryError
{
    private const string Prefix = "Category";

    // General errors
    public static readonly Error NullValue = Error.NullValue;

    // Validation errors
    public static readonly Error NameNullOrEmpty = Error.Validation(
        $"{Prefix}.NameNullOrEmpty",
        "Category name cannot be null or empty.");

    public static readonly Error NameTooLong = Error.Validation(
        $"{Prefix}.NameTooLong",
        $"Category name cannot exceed {CategoryName.MaxLength} characters.");

    public static readonly Error DescriptionNullOrEmpty = Error.Validation(
        $"{Prefix}.DescriptionNullOrEmpty",
        "Category description cannot be null or empty.");

    public static readonly Error DescriptionTooLong = Error.Validation(
        $"{Prefix}.DescriptionTooLong",
        $"Category description cannot exceed {CategoryDescription.MaxLength} characters.");

    // Post-related errors
    public static readonly Error PostNull = Error.Validation(
        $"{Prefix}.PostNull",
        "Post cannot be null.");

    public static readonly Error PostAlreadyExists = Error.Conflict(
        $"{Prefix}.PostAlreadyExists",
        "Post already exists in this category.");

    public static readonly Error PostNotFound = Error.NotFound(
        $"{Prefix}.PostNotFound",
        "Post does not exist in the category.");

    public static readonly Error UnexpectedError = Error.Unexpected(
        $"{Prefix}.UnexpectedError",
        "An unexpected error occurred.");

    public static readonly Error NotFound = Error.NotFound(
        $"{Prefix}.NotFound",
        "The category with the specified ID was not found.");

    public static readonly Error NameAlreadyExists =
        Error.Conflict($"{Prefix}.NameAlreadyExists",
            "A category with the same name already exists.");
}