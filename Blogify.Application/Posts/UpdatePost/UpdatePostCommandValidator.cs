using Blogify.Domain.Posts;
using FluentValidation;

namespace Blogify.Application.Posts.UpdatePost;

internal  sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        // Validate Title
        RuleFor(x => x.Title)
            .NotNull().WithMessage(PostErrors.TitleEmpty.Description);

        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage(PostErrors.TitleEmpty.Description)
            .MaximumLength(200).WithMessage(PostErrors.TitleTooLong.Description);

        // Validate Content
        RuleFor(x => x.Content)
            .NotNull().WithMessage(PostErrors.ContentEmpty.Description);

        RuleFor(x => x.Content.Value)
            .NotEmpty().WithMessage(PostErrors.ContentEmpty.Description)
            .MinimumLength(100).WithMessage(PostErrors.ContentTooShort.Description);

        // Validate Excerpt
        RuleFor(x => x.Excerpt)
            .NotNull().WithMessage(PostErrors.ExcerptEmpty.Description);

        RuleFor(x => x.Excerpt.Value)
            .NotEmpty().WithMessage(PostErrors.ExcerptEmpty.Description)
            .MaximumLength(500).WithMessage(PostErrors.ExcerptTooLong.Description);

        // // Validate CategoryId
        // RuleFor(x => x.CategoryId)
        //     .NotEmpty().WithMessage(PostErrors.CategoryIdEmpty.Description);
    }
}