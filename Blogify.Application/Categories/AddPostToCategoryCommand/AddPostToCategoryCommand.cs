using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Categories.AddPostToCategoryCommand;

public sealed record AddPostToCategoryCommand(Guid CategoryId, Guid PostId) : ICommand;
