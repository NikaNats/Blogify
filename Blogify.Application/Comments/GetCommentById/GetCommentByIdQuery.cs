using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Comments.GetCommentById;

public sealed record GetCommentByIdQuery(Guid Id) : IQuery<CommentResponse>;