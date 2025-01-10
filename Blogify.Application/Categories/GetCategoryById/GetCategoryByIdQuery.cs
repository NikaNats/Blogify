using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryByIdResponse>;