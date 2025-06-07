using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryByIdResponse>;