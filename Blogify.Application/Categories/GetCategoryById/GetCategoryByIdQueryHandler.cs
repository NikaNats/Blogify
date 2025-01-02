using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryByIdResponse>>
{
    public async Task<Result<CategoryByIdResponse>> Handle(GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return Result.Failure<CategoryByIdResponse>(Error.NotFound("Category.NotFound", "Category not found."));

            var response = new CategoryByIdResponse(
                category.Id,
                category.Name,
                category.Description,
                category.CreatedAt,
                category.UpdatedAt);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            // Log the exception if necessary
            return Result.Failure<CategoryByIdResponse>(
                Error.Unexpected("Category.UnexpectedError", ex.Message));
        }
    }
}