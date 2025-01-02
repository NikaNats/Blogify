using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Tags.GetAllTags;

public sealed record GetAllTagsQuery : IRequest<Result<List<AllTagResponse>>>;