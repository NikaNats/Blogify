using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.GetPostById;

public sealed record GetPostByIdQuery(Guid Id) : IQuery<PostResponse>;