using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Blogify.Domain.Users.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blogify.Application.Users.RegisterUser;

internal sealed class UserPendingRegistrationDomainEventHandler(
    IAuthenticationService authenticationService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserPendingRegistrationDomainEventHandler> logger)
    : INotificationHandler<UserPendingRegistrationDomainEvent>
{
    public async Task Handle(UserPendingRegistrationDomainEvent notification, CancellationToken cancellationToken)
    {
        // Load tracked user entity
        var user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Pending registration user {UserId} not found", notification.UserId);
            return; // swallow - message marked success to avoid endless retries on missing user
        }

        if (user.Status == UserStatus.Active)
        {
            logger.LogDebug("User {UserId} already active. Skipping external provisioning.", user.Id);
            return;
        }

        try
        {
            var identityId = await authenticationService.RegisterAsync(user, notification.Password, cancellationToken);
            user.Activate(identityId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("User {UserId} external identity provisioned and activated", user.Id);
        }
        catch (Exception ex)
        {
            // Rethrow with additional context so the outbox processor can log & retry.
            logger.LogError(ex, "Failed external provisioning for user {UserId}. Will retry.", user.Id);
            throw new InvalidOperationException($"Failed to provision external identity for user {user.Id}", ex);
        }
    }
}
