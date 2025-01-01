using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.GetAllCategories;

public sealed class
    GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryResponse>>>
{
    public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var categories = await categoryRepository.GetAllAsync(cancellationToken);
            var response = categories.Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.CreatedAt,
                category.UpdatedAt)).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            // Log the exception if necessary
            return Result.Failure<List<CategoryResponse>>(Error.Unexpected("Category.UnexpectedError", ex.Message));
        }
    }
}