using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AssignCategoriesToPost;

internal sealed class AssignCategoriesToPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository)
    : ICommandHandler<AssignCategoriesToPostCommand>
{
    public async Task<Result> Handle(AssignCategoriesToPostCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the post
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        // Retrieve and validate categories
        var categories = new List<Category>();
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
                return Result.Failure(CategoryError.NotFound);

            categories.Add(category);
        }

        // Assign categories to the post
        foreach (var category in categories)
        {
            var result = post.AddCategory(category);
            if (result.IsFailure)
                return result;
        }

        // Save changes
        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}