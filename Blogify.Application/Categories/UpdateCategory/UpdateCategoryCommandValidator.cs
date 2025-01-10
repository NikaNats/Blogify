using FluentValidation;

namespace Blogify.Application.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).ValidCategoryName();

        RuleFor(x => x.Description).ValidCategoryDescription();
    }
}