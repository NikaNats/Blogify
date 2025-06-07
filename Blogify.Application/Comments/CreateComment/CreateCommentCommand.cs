using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Comments.CreateComment;

public sealed record CreateCommentCommand(string Content, Guid AuthorId, Guid PostId) : ICommand<Guid>;