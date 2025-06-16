using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.RemoveCategoryFromPost;

internal sealed class RemoveCategoryFromPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) // Added IUnitOfWork
    : ICommandHandler<RemoveCategoryFromPostCommand>
{
    public async Task<Result> Handle(RemoveCategoryFromPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryError.NotFound);

        // CORRECT: Use the new method on the Post aggregate.
        var result = post.RemoveFromCategory(category);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}