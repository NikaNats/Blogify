using FluentValidation;

namespace Blogify.Application.Tags.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name cannot be empty.")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters.");
    }
}