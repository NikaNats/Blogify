using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.UpdateTag;

internal sealed class UpdateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTagCommand>
{
    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null) return Result.Failure(TagErrors.NotFound);

        var existingTagWithSameName = await tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTagWithSameName is not null && existingTagWithSameName.Id != tag.Id)
            return Result.Failure(TagErrors.DuplicateName);

        var nameResult = TagName.Create(request.Name);
        if (nameResult.IsFailure) return Result.Failure(nameResult.Error);

        tag.UpdateName(nameResult.Value.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}