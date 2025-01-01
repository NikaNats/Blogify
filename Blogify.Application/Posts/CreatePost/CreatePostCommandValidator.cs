using FluentValidation;

namespace Blogify.Application.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title.Value) // Access the underlying string value
            .NotNull().WithMessage("Post title cannot be null.")
            .NotEmpty().WithMessage("Post title cannot be empty.")
            .MaximumLength(200).WithMessage("Post title cannot be longer than 200 characters.");

        RuleFor(x => x.Content.Value) // Similarly for PostContent
            .NotNull().WithMessage("Post content cannot be null.")
            .NotEmpty().WithMessage("Post content cannot be empty.")
            .MinimumLength(100).WithMessage("Post content must be at least 100 characters long.");

        RuleFor(x => x.Excerpt.Value) // Similarly for PostExcerpt
            .NotNull().WithMessage("Post excerpt cannot be null.")
            .NotEmpty().WithMessage("Post excerpt cannot be empty.")
            .MaximumLength(500).WithMessage("Post excerpt cannot be longer than 500 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId cannot be empty.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId cannot be empty.");
    }
}