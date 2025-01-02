using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed record GetAllPostsQuery : IRequest<Result<List<AllPostResponse>>>;