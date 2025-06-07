using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Comments.DeleteComment;

internal sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage(CommentError.EmptyCommentId.Description);
    }
}