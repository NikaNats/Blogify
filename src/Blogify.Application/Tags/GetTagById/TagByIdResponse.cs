namespace Blogify.Application.Tags.GetTagById;

public sealed record TagByIdResponse(Guid Id, string Name, DateTimeOffset CreatedAt);