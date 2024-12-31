using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Tags.CreateTag;

public sealed class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly ITagRepository _tagRepository;

    public CreateTagCommandHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tagResult = Tag.Create(request.Name);
        if (tagResult.IsFailure)
            return Result.Failure<Guid>(tagResult.Error);

        var tag = tagResult.Value;
        await _tagRepository.AddAsync(tag, cancellationToken);

        return Result.Success(tag.Id);
    }
}