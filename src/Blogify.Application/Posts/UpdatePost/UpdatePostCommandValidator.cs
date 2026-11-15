using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.UpdatePost;

internal sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(PostErrors.PostIdEmpty.Description);

        // --- FIX: Validate the primitive string 'Title' directly ---
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
            .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description);

        // --- FIX: Validate the primitive string 'Content' directly ---
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MinimumLength(PostContent.MinLength).WithMessage(PostErrors.ContentTooShort.Description)
            .MaximumLength(PostContent.MaxLength).WithMessage(PostErrors.ContentTooLong.Description);

        // --- FIX: Validate the primitive string 'Excerpt' directly ---
        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description);
    }
}