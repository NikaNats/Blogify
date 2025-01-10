using FluentValidation;

namespace Blogify.Application.Categories.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).ValidCategoryName();

        RuleFor(x => x.Description).ValidCategoryDescription();
    }
}