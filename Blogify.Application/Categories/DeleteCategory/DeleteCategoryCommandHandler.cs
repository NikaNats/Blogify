using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.DeleteCategory;


internal sealed class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

            await categoryRepository.DeleteAsync(category, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            return Result.Failure(CategoryError.UnexpectedError);
        }
    }
}