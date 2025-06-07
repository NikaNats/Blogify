using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.GetTagById;

public sealed record GetTagByIdQuery(Guid Id) : IQuery<TagByIdResponse>;