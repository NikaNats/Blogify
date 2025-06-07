using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;

namespace Blogify.Application.Posts.GetPostsByAuthorId;

public sealed record GetPostsByAuthorIdQuery(Guid AuthorId) : IQuery<List<PostResponse>>;