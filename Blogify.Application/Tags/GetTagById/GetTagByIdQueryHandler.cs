using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Tags.GetTagById;

public sealed class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, Result<TagByIdResponse>>
{
    private readonly ITagRepository _tagRepository;

    public GetTagByIdQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<TagByIdResponse>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure<TagByIdResponse>(Error.NotFound("Tag.NotFound", "Tag not found."));

        var response = new TagByIdResponse(tag.Id, tag.Name.Value, tag.CreatedAt);
        return Result.Success(response);
    }
}