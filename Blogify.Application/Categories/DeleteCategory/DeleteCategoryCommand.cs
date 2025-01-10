using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
