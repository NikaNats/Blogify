using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.CreatePost;

internal sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
            .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MinimumLength(100).WithMessage(PostErrors.ContentTooShort.Description)
            .MaximumLength(5000).WithMessage(PostErrors.ContentTooLong.Description);

        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description);
    }
}