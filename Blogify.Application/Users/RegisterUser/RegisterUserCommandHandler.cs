using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Blogify.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IAuthenticationService authenticationService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var firstNameResult = FirstName.Create(request.FirstName);
        var lastNameResult = LastName.Create(request.LastName);
        var emailResult = Email.Create(request.Email);

        if (firstNameResult.IsFailure)
            return Result.Failure<Guid>(firstNameResult.Error);
        if (lastNameResult.IsFailure)
            return Result.Failure<Guid>(lastNameResult.Error);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var userResult = User.Create(
            firstNameResult.Value,
            lastNameResult.Value,
            emailResult.Value);

        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        var user = userResult.Value;

        try
        {
            var identityId = await authenticationService.RegisterAsync(
                user,
                request.Password,
                cancellationToken);

            user.SetIdentityId(identityId);

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register user {Email} in database after authentication", request.Email);

            if (user.IdentityId != null)
            {
                try
                {
                    await authenticationService.DeleteIdentityAsync(user.IdentityId, cancellationToken);
                    logger.LogInformation("Rolled back identity {IdentityId} due to persistence failure", user.IdentityId);
                }
                catch (Exception rollbackEx)
                {
                    logger.LogError(rollbackEx, "Failed to rollback identity {IdentityId} for user {Email}", user.IdentityId, request.Email);
                    // Manual intervention may be required if rollback fails
                }
            }

            return Result.Failure<Guid>(Error.Failure("User.Registration.Failed",
                "Failed to register user due to an unexpected error."));
        }
    }
}