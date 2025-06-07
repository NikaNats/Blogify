using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;

namespace Blogify.Application.Comments.CreateComment;

internal sealed class CreateCommentCommandHandler(ICommentRepository commentRepository, IPostRepository postRepository)
    : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var postExists = await postRepository.ExistsAsync(p => p.Id == request.PostId, cancellationToken);
        if (!postExists) return Result.Failure<Guid>(PostErrors.NotFound);
        
        var commentResult = Comment.Create(request.Content, request.AuthorId, request.PostId);
        if (commentResult.IsFailure)
            return Result.Failure<Guid>(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);

        return Result.Success(commentResult.Value.Id);
    }
}