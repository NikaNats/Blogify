using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.CreateTag;

public sealed class CreateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateTagCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tagResult = Tag.Create(request.Name);
        if (tagResult.IsFailure)
            return Result.Failure<Guid>(tagResult.Error);

        // Assign the tag value to a variable before using it
        var tag = tagResult.Value;

        // Check for duplicate tag names
        var existingTag = await tagRepository.GetByNameAsync(tag.Name.Value, cancellationToken);
        if (existingTag != null)
            return Result.Failure<Guid>(TagErrors.DuplicateName);

        await tagRepository.AddAsync(tag, cancellationToken);

        // Save changes to the database
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tag.Id);
    }
}