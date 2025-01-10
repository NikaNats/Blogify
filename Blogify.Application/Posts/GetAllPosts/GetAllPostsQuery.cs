using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed record GetAllPostsQuery : IQuery<List<AllPostResponse>>;