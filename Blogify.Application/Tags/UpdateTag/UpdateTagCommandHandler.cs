using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.UpdateTag;

internal sealed class UpdateTagCommandHandler(ITagRepository tagRepository) : ICommandHandler<UpdateTagCommand>
{
    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the tag by ID
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        // Update the tag's name
        var nameResult = TagName.Create(request.Name);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        tag.UpdateName(nameResult.Value.Value);

        // Save the changes
        await tagRepository.UpdateAsync(tag, cancellationToken);

        return Result.Success();
    }
}