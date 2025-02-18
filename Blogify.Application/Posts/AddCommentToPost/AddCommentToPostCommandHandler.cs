using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AddCommentToPost;

internal sealed class AddCommentToPostCommandHandler(IPostRepository postRepository,ICommentRepository commentRepository)
    : ICommandHandler<AddCommentToPostCommand>
{
    public async Task<Result> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var commentResult = post.AddComment(request.Content, request.AuthorId);
        if (commentResult.IsFailure)
            return Result.Failure(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);
        
        await postRepository.UpdateAsync(post, cancellationToken);
        
        return Result.Success();
    }
}