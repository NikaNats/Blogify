using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.GetPostById;

public sealed record GetPostByIdQuery(Guid Id) : IRequest<Result<PostResponse>>;