using Blogify.Domain.Tags;
using FluentValidation;

namespace Blogify.Application.Tags.CreateTag;

internal  sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(TagErrors.NameEmpty.Description)
            .MaximumLength(50).WithMessage(TagErrors.NameTooLong.Description);
    }
}