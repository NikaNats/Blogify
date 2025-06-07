using Blogify.Application.Abstractions.Messaging;
using MediatR;

namespace Blogify.Application.Comments.UpdateComment;

public sealed record UpdateCommentCommand(
    Guid RouteId,
    Guid CommentId,
    string Content
) : ICommand<Unit>;