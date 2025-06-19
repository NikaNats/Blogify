using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Comments.CreateComment;

public sealed record CreateCommentCommand(Guid PostId, string Content) : ICommand<Guid>;