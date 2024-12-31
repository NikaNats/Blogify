using FluentValidation;

namespace Blogify.Application.Posts.UpdatePost;

public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().WithMessage("Post title cannot be null.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Post content cannot be null.");

        RuleFor(x => x.Excerpt)
            .NotNull().WithMessage("Post excerpt cannot be null.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId cannot be empty.");
    }
}