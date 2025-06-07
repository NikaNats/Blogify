using FluentValidation;

namespace Blogify.Application.Categories.GetCategoryById;

internal sealed class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("Category ID cannot be empty.");
    }
}