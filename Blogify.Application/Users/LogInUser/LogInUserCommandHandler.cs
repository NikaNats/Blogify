using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;

namespace Blogify.Application.Users.LogInUser;

internal sealed class LogInUserCommandHandler(IJwtService jwtService)
    : ICommandHandler<LogInUserCommand, AccessTokenResponse>
{
    public async Task<Result<AccessTokenResponse>> Handle(
        LogInUserCommand request,
        CancellationToken cancellationToken)
    {
        Result<string> result = await jwtService.GetAccessTokenAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.IsFailure) return Result.Failure<AccessTokenResponse>(UserErrors.InvalidCredentials);

        return new AccessTokenResponse(result.Value);
    }
}