using Blogify.Application.Abstractions.Messaging;
using MediatR;

namespace Blogify.Application.Comments.UpdateComment;

public sealed record UpdateCommentCommand(
    Guid CommentId,
    string Content,
    Guid AuthorId
    // Guid PostId
    ) : ICommand<Unit>;