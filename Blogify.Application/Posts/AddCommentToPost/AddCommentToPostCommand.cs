using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed record AddCommentToPostCommand(Guid PostId, string Content, Guid AuthorId) : IRequest<Result>;