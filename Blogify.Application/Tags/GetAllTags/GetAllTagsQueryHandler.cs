using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.GetAllTags;

internal sealed class GetAllTagsQueryHandler(ITagRepository tagRepository)
    : IQueryHandler<GetAllTagsQuery, List<AllTagResponse>>
{
    public async Task<Result<List<AllTagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await tagRepository.GetAllAsync(cancellationToken);
        var response = tags.Select(tag => new AllTagResponse(tag.Id, tag.Name.Value, tag.CreatedAt)).ToList();
        return Result.Success(response);
    }
}