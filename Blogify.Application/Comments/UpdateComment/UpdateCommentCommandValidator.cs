using FluentValidation;
using Blogify.Domain.Comments;

namespace Blogify.Application.Comments.UpdateComment;

internal sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage(CommentError.EmptyCommentId.Description);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(CommentError.EmptyContent.Description)
            .MaximumLength(500).WithMessage(CommentError.ContentTooLong.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(CommentError.EmptyAuthorId.Description);

        // RuleFor(x => x.PostId)
        //     .NotEmpty().WithMessage(CommentError.EmptyPostId.Description);
    }
}