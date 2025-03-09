using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.UpdatePost;

internal sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(PostErrors.PostIdEmpty.Description);
        
        // Validate Title
        RuleFor(x => x.Title)
            .NotNull().WithMessage(PostErrors.TitleEmpty.Description);

        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
            .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description)
            .When(x => x.Title != null);

        // Validate Content
        RuleFor(x => x.Content)
            .NotNull().WithMessage(PostErrors.ContentEmpty.Description);

        RuleFor(x => x.Content.Value)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MinimumLength(100).WithMessage(PostErrors.ContentTooShort.Description)
            .When(x => x.Content != null);

        // Validate Excerpt
        RuleFor(x => x.Excerpt)
            .NotNull().WithMessage(PostErrors.ExcerptEmpty.Description);

        RuleFor(x => x.Excerpt.Value)
            .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description)
            .When(x => x.Excerpt != null);
    }
}