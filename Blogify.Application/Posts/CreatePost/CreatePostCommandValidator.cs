using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.CreatePost;

internal  sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
            .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description);

        RuleFor(x => x.Content.Value)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MinimumLength(100).WithMessage(PostErrors.ContentTooShort.Description);

        RuleFor(x => x.Excerpt.Value)
            .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(PostErrors.AuthorIdEmpty.Description);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage(PostErrors.CategoryIdEmpty.Description);
    }
}