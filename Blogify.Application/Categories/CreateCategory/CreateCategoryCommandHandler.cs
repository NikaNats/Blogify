using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.CreateCategory;

internal sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for duplicate name in the application layer
            var existingCategory = await categoryRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingCategory != null)
            {
                return Result.Failure<Guid>(CategoryError.NameAlreadyExists);
            }
            
            var categoryResult = Category.Create(request.Name, request.Description);
            if (categoryResult.IsFailure)
                return Result.Failure<Guid>(categoryResult.Error);

            var category = categoryResult.Value;
            await categoryRepository.AddAsync(category, cancellationToken);

            return Result.Success(category.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(CategoryError.UnexpectedError);
        }
    }
}