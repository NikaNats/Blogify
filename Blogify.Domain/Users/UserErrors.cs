using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = Error.Failure(
        "User.InvalidCredentials",
        "The provided credentials were invalid");

    public static readonly Error InvalidFirstName = Error.Failure(
        "User.InvalidFirstName",
        "First name cannot be empty.");

    public static readonly Error InvalidLastName = Error.Failure(
        "User.InvalidLastName",
        "Last name cannot be empty.");

    public static readonly Error InvalidEmail = Error.Failure(
        "User.InvalidEmail",
        "Email is invalid.");

    public static Error NotFound(Guid userId)
    {
        return Error.NotFound(
            "User.Found",
            $"The user with the Id = '{userId}' was not found");
    }
}