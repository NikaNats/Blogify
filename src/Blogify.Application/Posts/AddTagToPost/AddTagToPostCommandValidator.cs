using FluentValidation;

namespace Blogify.Application.Posts.AddTagToPost;

internal sealed class AddTagToPostCommandValidator : AbstractValidator<AddTagToPostCommand>
{
    public AddTagToPostCommandValidator()
    {
        RuleFor(x => x.TagId)
            .NotEmpty()
            .WithMessage("TagId cannot be empty.")
            .NotEqual(Guid.Empty)
            .WithMessage("TagId cannot be a default GUID.");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId cannot be empty.")
            .NotEqual(Guid.Empty)
            .WithMessage("PostId cannot be a default GUID.");
    }
}