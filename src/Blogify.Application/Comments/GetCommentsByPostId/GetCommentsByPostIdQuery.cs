using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed record GetCommentsByPostIdQuery(Guid PostId) : IQuery<List<CommentResponse>>;