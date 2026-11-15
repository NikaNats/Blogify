using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.GetAllTags;

internal sealed class GetAllTagsQueryHandler(ITagRepository tagRepository)
    : IQueryHandler<GetAllTagsQuery, PagedResponse<AllTagResponse>>
{
    public async Task<Result<PagedResponse<AllTagResponse>>> Handle(GetAllTagsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await tagRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(PagedResponse<AllTagResponse>.From(page, request.Page, request.PageSize,
            tag => new AllTagResponse(tag.Id, tag.Name.Value, tag.CreatedAt)));
    }
}
