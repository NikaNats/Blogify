using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using MediatR;

namespace Blogify.Application.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(request.Name, request.Description);
        if (categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        var category = categoryResult.Value;
        await categoryRepository.AddAsync(category, cancellationToken);

        return Result.Success(category.Id);
    }
}