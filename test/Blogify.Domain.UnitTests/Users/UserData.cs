using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;

namespace Blogify.Domain.UnitTests.Users;

internal static class UserData
{
    public const string IdentityId = "auth0|123456";

    public const string SecondIdentityId = "auth0|789012";

    // Predefined roles
    public static readonly Role Registered = Role.Registered;
    public static readonly Role Administrator = Role.Administrator;
    public static readonly Role Premium = Role.Premium;

    // Predefined user data
    public static readonly FirstName DefaultFirstName = FirstName.Create("John").Value;
    public static readonly LastName DefaultLastName = LastName.Create("Doe").Value;
    public static readonly Email DefaultEmail = Email.Create("john.doe@example.com").Value;

    // Test-specific data
    public static readonly Email NewEmail = Email.Create("new.email@example.com").Value;

    // Invalid test data
    public static readonly Result<Email>[] InvalidEmails =
    [
        Email.Create(""),
        Email.Create("invalid-email"),
        Email.Create(null),
        Email.Create("@example.com"),
        Email.Create("user@")
    ];

    public static readonly string[] InvalidNames =
    [
        "",
        null,
        "   "
    ];
}