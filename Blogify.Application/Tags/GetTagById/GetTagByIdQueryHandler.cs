using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.GetTagById;

public sealed class GetTagByIdQueryHandler(ITagRepository tagRepository)
    : IQueryHandler<GetTagByIdQuery, TagByIdResponse>
{
    public async Task<Result<TagByIdResponse>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure<TagByIdResponse>(TagErrors.NotFound);

        var response = new TagByIdResponse(tag.Id, tag.Name.Value, tag.CreatedAt);
        return Result.Success(response);
    }
}