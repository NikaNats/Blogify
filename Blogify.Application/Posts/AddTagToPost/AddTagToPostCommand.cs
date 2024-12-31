using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.AddTagToPost;

public sealed record AddTagToPostCommand(Guid PostId, Guid TagId) : IRequest<Result>;