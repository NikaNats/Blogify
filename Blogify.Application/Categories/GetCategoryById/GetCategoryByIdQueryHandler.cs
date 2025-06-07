using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;

namespace Blogify.Application.Categories.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetCategoryByIdQuery, CategoryByIdResponse>
{
    public async Task<Result<CategoryByIdResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null) return Result.Failure<CategoryByIdResponse>(CategoryError.NotFound);

            var response = new CategoryByIdResponse(
                category.Id,
                category.Name.Value,
                category.Description.Value,
                category.CreatedAt,
                category.LastModifiedAt);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            // Log the exception if necessary
            return Result.Failure<CategoryByIdResponse>(CategoryError.UnexpectedError);
        }
    }
}