using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = new(
        "User.InvalidCredentials",
        "The provided credentials were invalid",
        ErrorType.AuthenticationFailed);

    public static readonly Error InvalidFirstName = Error.Validation(
        "User.InvalidFirstName",
        "First name cannot be empty");

    public static readonly Error InvalidLastName = Error.Validation(
        "User.InvalidLastName",
        "Last name cannot be empty");

    public static readonly Error InvalidEmail = Error.Validation(
        "User.InvalidEmail",
        "Email address format is invalid");

    public static readonly Error EmailTooLong = Error.Validation(
        "User.EmailTooLong",
        "Email address cannot exceed 254 characters");

    public static readonly Error FirstNameTooLong = Error.Validation(
        "User.FirstNameTooLong",
        "First name cannot exceed 50 characters");

    public static readonly Error LastNameTooLong = Error.Validation(
        "User.LastNameTooLong",
        "Last name cannot exceed 50 characters");

    public static readonly Error CannotRemoveRegisteredRole = Error.Validation(
        "User.CannotRemoveRegisteredRole",
        "Cannot remove the Registered role from a user");

    public static readonly Error UserNotFound = Error.NotFound(
        "User.NotFound",
        "The specified user was not found");
}