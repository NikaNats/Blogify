using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Comments.CreateComment;

internal sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(CommentError.EmptyContent.Description)
            .MinimumLength(1).WithMessage(CommentError.ContentTooShort.Description)
            .MaximumLength(1000).WithMessage(CommentError.ContentTooLong.Description);

        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage(CommentError.EmptyPostId.Description);
    }
}