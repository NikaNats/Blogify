using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Tags.GetTagById;

public sealed record GetTagByIdQuery(Guid Id) : IRequest<Result<TagResponse>>;