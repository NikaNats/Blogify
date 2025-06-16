using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AssignCategoryToPost;

internal sealed class AssignCategoryToPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignCategoryToPostCommand>
{
    public async Task<Result> Handle(AssignCategoryToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null) return Result.Failure(CategoryError.NotFound);

        var result = post.AssignToCategory(category);

        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}