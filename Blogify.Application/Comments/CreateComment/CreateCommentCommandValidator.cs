using FluentValidation;

namespace Blogify.Application.Comments.CreateComment;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(500).WithMessage("Comment content cannot exceed 500 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId cannot be empty.");

        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId cannot be empty.");
    }
}