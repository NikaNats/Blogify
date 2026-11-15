using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Cryptography;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Blogify.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISecretProtector secretProtector,
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

        var existingUser = await userRepository.GetByEmailAsync(emailResult.Value.Address, cancellationToken);
        if (existingUser is not null)
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);

        var user = userResult.Value;

    // Raise integration (pending registration) domain event containing required data for external provisioning.
    // The password is protected because the event is persisted in the outbox until processed.
    user.ScheduleRegistration(secretProtector.Protect(request.Password));

    await userRepository.AddAsync(user, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    logger.LogInformation("User {Email} queued for external identity provisioning (Pending)", request.Email);

    return Result.Success(user.Id);
    }
}