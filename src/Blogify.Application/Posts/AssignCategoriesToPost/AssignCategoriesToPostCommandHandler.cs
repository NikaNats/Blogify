using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AssignCategoriesToPost;

internal sealed class AssignCategoriesToPostCommandHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<AssignCategoriesToPostCommand>
{
    public async Task<Result> Handle(AssignCategoriesToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        if (!PostAccess.CanModify(post, userContext)) return Result.Failure(PostErrors.Forbidden);

        // One round trip for all categories instead of one query per id.
        var distinctIds = request.CategoryIds.Distinct().ToList();
        var categoriesToAssign = await categoryRepository.GetByIdsAsync(distinctIds, cancellationToken);
        if (categoriesToAssign.Count != distinctIds.Count) return Result.Failure(CategoryError.NotFound);

        foreach (var category in categoriesToAssign) post.AssignToCategory(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}