using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Description) : IRequest<Result<Guid>>;