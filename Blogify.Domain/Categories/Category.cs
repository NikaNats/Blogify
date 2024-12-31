using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories.Events;
using Blogify.Domain.Posts;

namespace Blogify.Domain.Categories;

public sealed class Category : Entity
{
    private readonly List<Post> _posts = new();

    private Category(Guid id, string name, string description)
        : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    private Category() { }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public static Result<Category> Create(string name, string description)
    {
        if (string.IsNullOrEmpty(name))
            return Result.Failure<Category>(Error.Validation("Category.Name", "Name cannot be empty."));

        if (string.IsNullOrEmpty(description))
            return Result.Failure<Category>(Error.Validation("Category.Description", "Description cannot be empty."));

        var category = new Category(Guid.NewGuid(), name, description);
        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        return Result.Success(category);
    }

    public Result Update(string name, string description)
    {
        if (string.IsNullOrEmpty(name))
            return Result.Failure(Error.Validation("Category.Name", "Name cannot be empty."));

        if (string.IsNullOrEmpty(description))
            return Result.Failure(Error.Validation("Category.Description", "Description cannot be empty."));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new CategoryUpdatedDomainEvent(Id));
        return Result.Success();
    }
}