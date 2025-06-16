using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;

namespace Blogify.Application.Categories.CreateCategory;

internal sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await categoryRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingCategory != null) return Result.Failure<Guid>(CategoryError.NameAlreadyExists);

        var categoryResult = Category.Create(request.Name, request.Description);
        if (categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        var category = categoryResult.Value;
        await categoryRepository.AddAsync(category, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}