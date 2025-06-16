using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AssignCategoriesToPost;

internal sealed class AssignCategoriesToPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignCategoriesToPostCommand>
{
    public async Task<Result> Handle(AssignCategoriesToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var categoriesToAssign = new List<Category>();
        foreach (var id in request.CategoryIds.Distinct())
        {
            var category = await categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category is null) return Result.Failure(CategoryError.NotFound);
            categoriesToAssign.Add(category);
        }

        foreach (var category in categoriesToAssign) post.AssignToCategory(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}