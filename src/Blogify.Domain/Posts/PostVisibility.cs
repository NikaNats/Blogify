namespace Blogify.Domain.Posts;

/// <summary>The minimum a caller needs to decide whether a post may be seen, without loading the aggregate.</summary>
public sealed record PostVisibility(PublicationStatus Status, Guid AuthorId);
