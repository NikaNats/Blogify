using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.GetAllCategories;

public sealed class
    GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetAllCategoriesQuery, List<AllCategoryResponse>>
{
    public async Task<Result<List<AllCategoryResponse>>> Handle(GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {

            var categories = await categoryRepository.GetAllAsync(cancellationToken);
            var response = categories.Select(category => new AllCategoryResponse(
                category.Id,
                category.Name.Value,
                category.Description.Value,
                category.CreatedAt,
                category.LastModifiedAt)).ToList();
            return Result.Success(response);
    }
}