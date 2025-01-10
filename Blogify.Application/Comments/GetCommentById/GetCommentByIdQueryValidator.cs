using Blogify.Domain.Comments;
using FluentValidation;

namespace Blogify.Application.Comments.GetCommentById;

public sealed class GetCommentByIdQueryValidator : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(CommentError.InvalidContent.Description);
    }
}