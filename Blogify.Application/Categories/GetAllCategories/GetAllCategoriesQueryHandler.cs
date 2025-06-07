using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;

namespace Blogify.Application.Categories.GetAllCategories;

internal sealed class
    GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetAllCategoriesQuery, List<AllCategoryResponse>>
{
    public async Task<Result<List<AllCategoryResponse>>> Handle(GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        try
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
        catch (Exception ex)
        {
            return Result.Failure<List<AllCategoryResponse>>(CategoryError.UnexpectedError);
        }
    }
}