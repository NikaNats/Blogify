using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Tags.GetAllTags;

public sealed class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<List<TagResponse>>>
{
    private readonly ITagRepository _tagRepository;

    public GetAllTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<List<TagResponse>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        var response = tags.Select(tag => new TagResponse(tag.Id, tag.Name, tag.CreatedAt)).ToList();
        return Result.Success(response);
    }
}