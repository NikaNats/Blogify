using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories.Events;

namespace Blogify.Domain.Categories;

/// <summary>
///     Represents the Category aggregate root. A Category is an independent entity
///     used for classifying posts. It does not own or manage the posts associated with it.
/// </summary>
public sealed class Category : AuditableEntity
{
    private Category(Guid id, CategoryName name, CategoryDescription description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    // Private constructor for EF Core.
    private Category()
    {
    }

    public CategoryName Name { get; private set; }
    public CategoryDescription Description { get; private set; }

    public static Result<Category> Create(string name, string description)
    {
        var categoryNameResult = CategoryName.Create(name);
        if (categoryNameResult.IsFailure)
            return Result.Failure<Category>(categoryNameResult.Error);

        var categoryDescriptionResult = CategoryDescription.Create(description);
        if (categoryDescriptionResult.IsFailure)
            return Result.Failure<Category>(categoryDescriptionResult.Error);

        var category = new Category(
            Guid.NewGuid(),
            categoryNameResult.Value,
            categoryDescriptionResult.Value
        );

        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id));

        return Result.Success(category);
    }

    public Result Update(string name, string description)
    {
        var categoryNameResult = CategoryName.Create(name);
        if (categoryNameResult.IsFailure) return Result.Failure(categoryNameResult.Error);

        var categoryDescriptionResult = CategoryDescription.Create(description);
        if (categoryDescriptionResult.IsFailure) return Result.Failure(categoryDescriptionResult.Error);

        // Only update and raise event if there's an actual change.
        if (Name.Equals(categoryNameResult.Value) && Description.Equals(categoryDescriptionResult.Value))
            return Result.Success();

        Name = categoryNameResult.Value;
        Description = categoryDescriptionResult.Value;

        RaiseDomainEvent(new CategoryUpdatedDomainEvent(Id));
        return Result.Success();
    }
}