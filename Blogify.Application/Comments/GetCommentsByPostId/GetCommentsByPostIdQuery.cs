using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed record GetCommentsByPostIdQuery(Guid PostId) : IQuery<List<CommentResponse>>;