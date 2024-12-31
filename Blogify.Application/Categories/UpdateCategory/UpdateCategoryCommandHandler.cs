using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        var updateResult = category.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return Result.Success();
    }
}