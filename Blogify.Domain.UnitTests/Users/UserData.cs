using Blogify.Domain.Users;

namespace Blogify.Domain.UnitTests.Users
{
    internal static class UserData
    {
        // Predefined roles
        public static readonly Role Registered = Role.Registered;
        public static readonly Role Administrator = Role.Administrator;
        public static readonly Role Premium = Role.Premium;

        // Predefined user data
        public static readonly FirstName DefaultFirstName = new("John");
        public static readonly LastName DefaultLastName = LastName.Create("Doe").Value;
        public static readonly string DefaultEmail = "john.doe@example.com";

        // Test-specific data
        public static readonly string NewEmail = "new.email@example.com";
        public static readonly string IdentityId = "auth0|123456";
        public static readonly string SecondIdentityId = "auth0|789012";

        // Invalid test data
        public static readonly string[] InvalidEmails =
        [
            "",
            "invalid-email",
            null,
            "@example.com",
            "user@"
        ];

        public static readonly string[] InvalidNames =
        [
            "",
            null,
            "   "
        ];
    }
}