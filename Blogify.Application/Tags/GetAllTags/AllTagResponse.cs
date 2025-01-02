namespace Blogify.Application.Tags.GetAllTags;

public sealed record AllTagResponse(Guid Id, string Name, DateTime CreatedAt);