using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Tags.CreateTag;

public sealed record CreateTagCommand(string Name) : IRequest<Result<Guid>>;