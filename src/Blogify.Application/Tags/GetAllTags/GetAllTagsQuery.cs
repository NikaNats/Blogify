using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.GetAllTags;

public sealed record GetAllTagsQuery : IQuery<List<AllTagResponse>>;