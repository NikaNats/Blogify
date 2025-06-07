using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string Description) : ICommand;