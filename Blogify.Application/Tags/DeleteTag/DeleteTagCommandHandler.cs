using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.DeleteTag;

public sealed class DeleteTagCommandHandler(ITagRepository tagRepository) : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the tag by ID
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        // Delete the tag
        await tagRepository.DeleteAsync(tag.Id, cancellationToken);

        return Result.Success();
    }
}