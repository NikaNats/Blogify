using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed record AddCommentToPostCommand(Guid PostId, string Content, Guid AuthorId) : ICommand;