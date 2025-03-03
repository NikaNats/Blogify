using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories.Events;
using Blogify.Domain.Posts;

namespace Blogify.Domain.Categories;

public sealed class Category : Entity
{
    private readonly List<Post> _posts = [];
    private CategoryDescription _description;
    private CategoryName _name;

    private Category(Guid id, CategoryName name, CategoryDescription description)
        : base(id)
    {
        _name = name;
        _description = description;
    }

    private Category()
    {
    }

    public CategoryName Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public CategoryDescription Description
    {
        get => _description;
        private set => SetProperty(ref _description, value);
    }
    
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

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
        var categoryDescriptionResult = CategoryDescription.Create(description);

        if (categoryNameResult.IsFailure)
            return Result.Failure(categoryNameResult.Error);

        if (categoryDescriptionResult.IsFailure)
            return Result.Failure(categoryDescriptionResult.Error);

        if (Name.Equals(categoryNameResult.Value) &&
            Description.Equals(categoryDescriptionResult.Value))
            return Result.Success();

        Name = categoryNameResult.Value;
        Description = categoryDescriptionResult.Value;

        RaiseDomainEvent(new CategoryUpdatedDomainEvent(Id));

        return Result.Success();
    }

    public Result AddPost(Post post)
    {
        if (post == null)
            return Result.Failure(CategoryError.PostNull);

        if (_posts.Contains(post))
            return Result.Failure(CategoryError.PostAlreadyExists);

        _posts.Add(post);

        UpdateModificationTimestamp();

        RaiseDomainEvent(new PostAddedToCategoryDomainEvent(Id, post.Id));

        return Result.Success();
    }

    public Result RemovePost(Post post)
    {
        if (post == null)
            return Result.Failure(CategoryError.PostNull);

        if (!_posts.Remove(post))
            return Result.Failure(CategoryError.PostNotFound);

        UpdateModificationTimestamp();

        RaiseDomainEvent(new PostRemovedFromCategoryDomainEvent(Id, post.Id));
        return Result.Success();
    }
}