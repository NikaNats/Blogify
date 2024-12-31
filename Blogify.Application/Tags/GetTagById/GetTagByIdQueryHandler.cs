using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Tags.GetTagById;

public sealed class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, Result<TagResponse>>
{
    private readonly ITagRepository _tagRepository;

    public GetTagByIdQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<TagResponse>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure<TagResponse>(Error.NotFound("Tag.NotFound", "Tag not found."));

        var response = new TagResponse(tag.Id, tag.Name, tag.CreatedAt);
        return Result.Success(response);
    }
}