using FluentValidation;

namespace Blogify.Application.Posts.RemoveTagFromPost;

internal sealed class RemoveTagFromPostCommandValidator : AbstractValidator<RemoveTagFromPostCommand>
{
    public RemoveTagFromPostCommandValidator()
    {
        // Rule for PostId
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("PostId cannot be a default GUID.");

        // Rule for TagId
        RuleFor(x => x.TagId)
            .NotEmpty().WithMessage("TagId cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("TagId cannot be a default GUID.");
    }
}