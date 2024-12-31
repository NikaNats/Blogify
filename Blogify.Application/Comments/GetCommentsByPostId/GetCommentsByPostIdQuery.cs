using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed record GetCommentsByPostIdQuery(Guid PostId) : IRequest<Result<List<CommentResponse>>>;