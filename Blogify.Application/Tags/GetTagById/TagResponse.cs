namespace Blogify.Application.Tags.GetTagById;

public sealed record TagResponse(Guid Id, string Name, DateTime CreatedAt);