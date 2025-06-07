using FluentValidation;

namespace Blogify.Application.Tags.DeleteTag;

internal sealed class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Tag ID cannot be empty.");
    }
}