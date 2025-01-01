using Blogify.Domain.Abstractions;
using Blogify.Domain.Users.Events;

namespace Blogify.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = new();

    private User(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    private User()
    {
    }

    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public string IdentityId { get; private set; } = string.Empty;

    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    public static Result<User> Create(FirstName firstName, LastName lastName, Email email)
    {
        if (string.IsNullOrEmpty(firstName.Value))
            return Result.Failure<User>(UserErrors.InvalidFirstName);

        if (string.IsNullOrEmpty(lastName.Value))
            return Result.Failure<User>(UserErrors.InvalidLastName);

        var emailResult = Email.Create(email.Address);

        if (emailResult.IsFailure)
            return Result.Failure<User>(emailResult.Error);

        var user = new User(Guid.NewGuid(), firstName, lastName, emailResult.Value);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));

        user.AddRole(Role.Registered);

        return Result.Success(user);
    }

    public void AddRole(Role role)
    {
        if (_roles.Contains(role)) return;

        _roles.Add(role);
        RaiseDomainEvent(new RoleAssignedDomainEvent(Id, role.Id));
    }

    public Result ChangeEmail(Email newEmail)
    {
        var emailResult = Email.Create(newEmail.Address);

        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (Email.Address == emailResult.Value.Address)
            return Result.Success();

        Email = emailResult.Value;
        RaiseDomainEvent(new EmailChangedDomainEvent(Id, Email.Address));

        return Result.Success();
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }
}