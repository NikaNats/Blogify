using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.RemoveTagFromPost;

public sealed record RemoveTagFromPostCommand(Guid PostId, Guid TagId) : IRequest<Result>;