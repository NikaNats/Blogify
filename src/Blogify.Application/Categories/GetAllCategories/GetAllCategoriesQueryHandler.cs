using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;

namespace Blogify.Application.Categories.GetAllCategories;

internal sealed class
    GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetAllCategoriesQuery, PagedResponse<AllCategoryResponse>>
{
    public async Task<Result<PagedResponse<AllCategoryResponse>>> Handle(GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await categoryRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(PagedResponse<AllCategoryResponse>.From(page, request.Page, request.PageSize,
            category => new AllCategoryResponse(
                category.Id,
                category.Name.Value,
                category.Description.Value,
                category.CreatedAt,
                category.LastModifiedAt)));
    }
}
