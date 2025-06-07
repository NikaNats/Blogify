using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Posts.AddCommentToPost;

internal sealed class AddCommentToPostCommandValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(CommentError.EmptyContent.Description)
            .MaximumLength(500).WithMessage(CommentError.ContentTooLong.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(CommentError.EmptyAuthorId.Description);
    }
}