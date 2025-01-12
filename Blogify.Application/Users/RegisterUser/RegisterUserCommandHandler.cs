using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;

namespace Blogify.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IAuthenticationService authenticationService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate and create domain objects
        var firstNameResult = FirstName.Create(request.FirstName);
        var lastNameResult = LastName.Create(request.LastName);
        var emailResult = Email.Create(request.Email);

        // Step 2: Check for failures in domain object creation
        if (firstNameResult.IsFailure)
            return Result.Failure<Guid>(firstNameResult.Error);

        if (lastNameResult.IsFailure)
            return Result.Failure<Guid>(lastNameResult.Error);

        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        // Step 3: Create the user
        var userResult = User.Create(
            firstNameResult.Value,
            lastNameResult.Value,
            emailResult.Value);

        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        var user = userResult.Value;

        // Step 4: Register the user in the authentication system
        var identityId = await authenticationService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);

        user.SetIdentityId(identityId);

        // Step 5: Add the user to the repository
        await userRepository.AddAsync(user, cancellationToken);

        // Step 6: Save changes to the database
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 7: Return the user ID
        return Result.Success(user.Id);
    }
}