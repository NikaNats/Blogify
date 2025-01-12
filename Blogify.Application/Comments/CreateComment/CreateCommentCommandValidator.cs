using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Comments.CreateComment;

internal sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(CommentError.InvalidContent.Description)
            .MaximumLength(500).WithMessage(CommentError.ContentTooLong.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(CommentError.EmptyAuthorId.Description);

        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage(CommentError.EmptyPostId.Description);
    }
}