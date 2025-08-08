using Blogify.Domain.Abstractions;
using Blogify.Domain.Users.Events;

namespace Blogify.Domain.Users;

public sealed class User : AuditableEntity
{
    private readonly List<Role> _roles = [];

    private User(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Status = UserStatus.Pending;
    }

    private User()
    {
    }

    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public Email Email { get; private set; }
    public string? IdentityId { get; private set; }
    public UserStatus Status { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public static Result<User> Create(FirstName firstName, LastName lastName, Email email)
    {
        if (string.IsNullOrWhiteSpace(firstName.Value))
            return Result.Failure<User>(UserErrors.InvalidFirstName);

        if (string.IsNullOrWhiteSpace(lastName.Value))
            return Result.Failure<User>(UserErrors.InvalidLastName);

        var emailResult = Email.Create(email.Address);

        if (emailResult.IsFailure)
            return Result.Failure<User>(emailResult.Error);

        var user = new User(Guid.NewGuid(), firstName, lastName, emailResult.Value);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
        // Also raise a pending registration event so that the outbox processor can
        // asynchronously provision the identity in the external auth provider.
        // The password is supplied later from the command handler using ScheduleRegistration.

        user.AddRole(Role.Registered);

        return Result.Success(user);
    }

    /// <summary>
    /// Schedules external identity registration by raising a domain event carrying the required data.
    /// Password is included in the outbox payload; ensure outbox retention / encryption policies mitigate exposure.
    /// </summary>
    public void ScheduleRegistration(string password)
    {
        RaiseDomainEvent(new UserPendingRegistrationDomainEvent(
            Id,
            Email.Address,
            FirstName.Value,
            LastName.Value,
            password));
    }

    public Result AddRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            return Result.Success();

        _roles.Add(role);

        RaiseDomainEvent(new RoleAssignedDomainEvent(
            Id,
            role.Id,
            role.Name));

        return Result.Success();
    }

    public Result ChangeEmail(Email newEmail)
    {
        var emailResult = Email.Create(newEmail.Address);

        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (Email.Address == emailResult.Value.Address)
            return Result.Success();

        var oldEmail = Email.Address;
        Email = emailResult.Value;

        RaiseDomainEvent(new EmailChangedDomainEvent(
            Id,
            oldEmail,
            newEmail.Address));

        return Result.Success();
    }

    public Result ChangeName(FirstName firstName, LastName lastName)
    {
        var changed = false;
        var oldFirstName = FirstName.Value;
        var oldLastName = LastName.Value;

        if (!FirstName.Value.Equals(firstName.Value))
        {
            FirstName = firstName;
            changed = true;
        }

        if (!LastName.Value.Equals(lastName.Value))
        {
            LastName = lastName;
            changed = true;
        }

        if (changed)
            RaiseDomainEvent(new UserNameChangedDomainEvent(
                Id,
                oldFirstName,
                oldLastName,
                FirstName.Value,
                LastName.Value));

        return Result.Success();
    }

    public void Activate(string identityId)
    {
        if (Status == UserStatus.Active)
        {
            if (IdentityId == null)
            {
                IdentityId = identityId; // fill missing
            }
            return; // already active
        }

        IdentityId = identityId;
        Status = UserStatus.Active;
        RaiseDomainEvent(new UserActivatedDomainEvent(Id));
    }
}