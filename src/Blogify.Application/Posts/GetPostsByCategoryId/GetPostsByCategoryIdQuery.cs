using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;

namespace Blogify.Application.Posts.GetPostsByCategoryId;

public sealed record GetPostsByCategoryIdQuery(Guid CategoryId) : IQuery<List<PostResponse>>;