namespace Blogify.Application.Tags.GetAllTags;

public sealed record TagResponse(Guid Id, string Name, DateTime CreatedAt);