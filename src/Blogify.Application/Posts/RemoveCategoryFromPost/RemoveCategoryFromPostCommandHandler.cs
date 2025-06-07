using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.RemoveCategoryFromPost;

internal sealed class RemoveCategoryFromPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository)
    : ICommandHandler<RemoveCategoryFromPostCommand>
{
    public async Task<Result> Handle(RemoveCategoryFromPostCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the post
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        // Retrieve the category
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryError.NotFound);

        // Remove the category from the post
        var result = post.RemoveCategory(category);
        if (result.IsFailure)
            return result;

        // Save changes
        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}