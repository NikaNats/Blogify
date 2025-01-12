using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));
        
        // Check if the name or description has changed
        if (category.Name.Value == request.Name && category.Description.Value == request.Description)
        {
            // No changes, return success without calling UpdateAsync
            return Result.Success();
        }
        var updateResult = category.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return Result.Success();
    }
}