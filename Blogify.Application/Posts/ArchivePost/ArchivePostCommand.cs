using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.ArchivePost;

public sealed record ArchivePostCommand(Guid Id) : IRequest<Result>;