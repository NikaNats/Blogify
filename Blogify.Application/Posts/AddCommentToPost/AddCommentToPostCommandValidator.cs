using FluentValidation;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed class AddCommentToPostCommandValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(500).WithMessage("Comment content cannot exceed 500 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId cannot be empty.");
    }
}