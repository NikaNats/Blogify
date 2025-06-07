using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Comments.GetCommentById;

public sealed record GetCommentByIdQuery(Guid Id) : IQuery<CommentResponse>;