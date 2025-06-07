using Blogify.Application.Abstractions.Messaging;
using MediatR;

namespace Blogify.Application.Comments.DeleteComment;

public sealed record DeleteCommentCommand(
    Guid CommentId) : ICommand<Unit>;