using Blogify.Domain.Tags;
using FluentValidation;

namespace Blogify.Application.Tags.UpdateTag;

internal sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(TagErrors.NotFound.Description);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(TagErrors.NameEmpty.Description)
            .MaximumLength(50).WithMessage(TagErrors.NameTooLong.Description);
    }
}