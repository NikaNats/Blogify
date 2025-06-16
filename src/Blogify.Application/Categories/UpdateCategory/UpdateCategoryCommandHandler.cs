using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Categories.Events;

namespace Blogify.Application.Categories.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryError.NotFound);

        if (category.Name.Value == request.Name && category.Description.Value == request.Description)
            return Result.Success();
        var updateResult = category.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        if (category.DomainEvents.Any(e => e is CategoryUpdatedDomainEvent))
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}