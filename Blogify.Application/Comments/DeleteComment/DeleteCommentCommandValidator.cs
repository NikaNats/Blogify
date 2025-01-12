using FluentValidation;
using Blogify.Domain.Comments;

namespace Blogify.Application.Comments.DeleteComment;

internal sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage(CommentError.EmptyCommentId.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(CommentError.EmptyAuthorId.Description);
    }
}