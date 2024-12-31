using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.PublishPost;

public sealed record PublishPostCommand(Guid Id) : IRequest<Result>;