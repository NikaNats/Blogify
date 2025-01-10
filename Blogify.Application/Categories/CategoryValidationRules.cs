using FluentValidation;

namespace Blogify.Application.Categories
{
    public static class CategoryValidationRules
    {
        public static IRuleBuilderOptions<T, string> ValidCategoryName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder
                .NotEmpty().WithMessage("Category name cannot be empty.")
                .MaximumLength(CategoryConstraints.NameMaxLength).WithMessage($"Category name cannot exceed {CategoryConstraints.NameMaxLength} characters.");

        public static IRuleBuilderOptions<T, string> ValidCategoryDescription<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder
                .NotEmpty().WithMessage("Category description cannot be empty.")
                .MaximumLength(CategoryConstraints.DescriptionMaxLength).WithMessage($"Category description cannot exceed {CategoryConstraints.DescriptionMaxLength} characters.");
    }
}

public static class CategoryConstraints
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
}