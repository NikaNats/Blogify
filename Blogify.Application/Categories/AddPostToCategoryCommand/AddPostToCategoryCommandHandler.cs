using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Categories.AddPostToCategoryCommand;

internal sealed class AddPostToCategoryCommandHandler(ICategoryRepository categoryRepository, IPostRepository postRepository)
    : IRequestHandler<AddPostToCategoryCommand, Result>
{
    public async Task<Result> Handle(AddPostToCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        var result = category.AddPost(post);
        if (result.IsFailure)
            return result;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return Result.Success();
    }
}