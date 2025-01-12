using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.AddCommentToPost;

internal sealed class AddCommentToPostCommandValidator : AbstractValidator<AddCommentToPostCommand>
{
    public AddCommentToPostCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ContentTooLong.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(PostErrors.AuthorIdEmpty.Description);
    }
}