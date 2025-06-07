using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;

namespace Blogify.Application.Posts.GetPostsByTagId;

public sealed record GetPostsByTagIdQuery(Guid TagId) : IQuery<List<PostResponse>>;