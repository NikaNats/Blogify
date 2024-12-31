using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Categories.GetAllCategories;

public sealed record GetAllCategoriesQuery : IRequest<Result<List<CategoryResponse>>>;