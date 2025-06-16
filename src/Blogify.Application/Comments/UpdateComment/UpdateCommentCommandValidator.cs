using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Comments.UpdateComment;

internal sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage(CommentError.EmptyCommentId.Description);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(CommentError.EmptyContent.Description)
            .MaximumLength(1000).WithMessage(CommentError.ContentTooLong.Description);
    }
}