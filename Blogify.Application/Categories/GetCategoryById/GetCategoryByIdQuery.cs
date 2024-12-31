using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryResponse>>;