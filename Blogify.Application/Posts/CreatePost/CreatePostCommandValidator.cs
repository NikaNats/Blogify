using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.CreatePost;

internal sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().WithMessage("Post title cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Title.Value)
                    .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
                    .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description);
            });

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Post content cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Content.Value)
                    .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
                    .MinimumLength(100).WithMessage(PostErrors.ContentTooShort.Description)
                    .MaximumLength(5000).WithMessage(PostErrors.ContentTooLong.Description);
            });

        RuleFor(x => x.Excerpt)
            .NotNull().WithMessage("Post excerpt cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Excerpt.Value)
                    .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
                    .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description);
            });
        
        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage(PostErrors.AuthorIdEmpty.Description);
    }
}