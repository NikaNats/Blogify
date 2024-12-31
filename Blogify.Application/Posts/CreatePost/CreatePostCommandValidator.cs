using FluentValidation;

namespace Blogify.Application.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().WithMessage("Post title cannot be null.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Post content cannot be null.");

        RuleFor(x => x.Excerpt)
            .NotNull().WithMessage("Post excerpt cannot be null.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId cannot be empty.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId cannot be empty.");
    }
}