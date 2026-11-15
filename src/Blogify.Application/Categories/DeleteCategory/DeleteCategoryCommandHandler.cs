using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Categories.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IPostRepository postRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryError.NotFound);

        await categoryRepository.DeleteAsync(category, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Posts store category ids as an array without a foreign key, so clear the dangling references.
        await postRepository.RemoveCategoryReferencesAsync(category.Id, cancellationToken);

        return Result.Success();
    }
}