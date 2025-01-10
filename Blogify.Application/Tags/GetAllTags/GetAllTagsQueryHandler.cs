using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;

namespace Blogify.Application.Tags.GetAllTags;

public sealed class GetAllTagsQueryHandler : IQueryHandler<GetAllTagsQuery, List<AllTagResponse>>
{
    private readonly ITagRepository _tagRepository;

    public GetAllTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<List<AllTagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        var response = tags.Select(tag => new AllTagResponse(tag.Id, tag.Name.Value, tag.CreatedAt)).ToList();
        return Result.Success(response);
    }
}