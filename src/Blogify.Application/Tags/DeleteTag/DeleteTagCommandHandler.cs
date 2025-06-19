using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.DeleteTag;

internal sealed class DeleteTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null) return Result.Failure(TagErrors.NotFound);

        await tagRepository.DeleteAsync(tag.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}