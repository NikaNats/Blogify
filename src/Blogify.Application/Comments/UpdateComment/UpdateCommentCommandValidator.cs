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

        RuleFor(x => x.CommentId)
            .Equal(x => x.RouteId)
            .WithMessage("Comment ID in the route does not match the Comment ID in the body.");
    }
}