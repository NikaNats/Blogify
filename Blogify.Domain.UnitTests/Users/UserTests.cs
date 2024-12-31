using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Blogify.Domain.Users.Events;
using FluentAssertions;
using Xunit;

namespace Blogify.Domain.UnitTests.Users;

public class UserTests
{
    public class CreateMethod
    {
        [Fact]
        public void Create_WithValidInputs_ShouldCreateUserWithDefaultRole()
        {
            // Arrange
            var firstName = UserData.DefaultFirstName;
            var lastName = UserData.DefaultLastName;
            var email = UserData.DefaultEmail;

            // Act
            var result = User.Create(firstName, lastName, email);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().NotBe(Guid.Empty);
            result.Value.FirstName.Should().Be(firstName);
            result.Value.LastName.Should().Be(lastName);
            result.Value.Email.Address.Should().Be(email);
            result.Value.IdentityId.Should().BeEmpty();
            result.Value.Roles.Should().ContainSingle(r => r == UserData.Registered);
        }

        [Fact]
        public void Create_WithValidInputs_ShouldRaiseUserCreatedAndRoleAssignedEvents()
        {
            // Act
            var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail);

            // Assert
            var events = result.Value.GetDomainEvents();
            events.Should().HaveCount(2);

            events.Should().ContainSingle(e => e is UserCreatedDomainEvent)
                .Which.Should().BeOfType<UserCreatedDomainEvent>()
                .Which.UserId.Should().Be(result.Value.Id);

            events.Should().ContainSingle(e => e is RoleAssignedDomainEvent)
                .Which.Should().BeOfType<RoleAssignedDomainEvent>()
                .Which.Should().Match<RoleAssignedDomainEvent>(e =>
                    e.UserId == result.Value.Id &&
                    e.RoleId == UserData.Registered.Id);
        }

        [Theory]
        [InlineData(0)] // Index for ""
        [InlineData(1)] // Index for null
        public void Create_WithInvalidLastName_ShouldReturnInvalidLastNameError(int index)
        {
            // Arrange
            var invalidLastNameInput = UserData.InvalidNames[index];

            // Act
            var lastNameResult = LastName.Create(invalidLastNameInput);

            // Assert
            lastNameResult.IsFailure.Should().BeTrue();
            lastNameResult.Error.Should().Be(UserErrors.InvalidLastName);
        }

        [Theory]
        [InlineData(0)] // Index for ""
        [InlineData(1)] // Index for "invalid-email"
        [InlineData(2)] // Index for null
        [InlineData(3)] // Index for "@example.com"
        [InlineData(4)] // Index for "user@"
        public void Create_WithInvalidEmail_ShouldReturnInvalidEmailError(int index)
        {
            // Act
            var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.InvalidEmails[index]);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UserErrors.InvalidEmail);
        }
    }

    public class AddRoleMethod
    {
        [Fact]
        public void AddRole_WithNewRole_ShouldAddRoleAndRaiseRoleAssignedEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var newRole = UserData.Administrator;

            // Act
            user.AddRole(newRole);

            // Assert
            user.Roles.Should().Contain(newRole);
            var events = user.GetDomainEvents();
            events.Should().ContainSingle()
                .Which.Should().BeOfType<RoleAssignedDomainEvent>()
                .Which.Should().Match<RoleAssignedDomainEvent>(e =>
                    e.UserId == user.Id &&
                    e.RoleId == newRole.Id);
        }

        [Fact]
        public void AddRole_WithExistingRole_ShouldNotAddDuplicateRoleOrRaiseEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var existingRole = UserData.Registered;

            // Act
            user.AddRole(existingRole);

            // Assert
            user.Roles.Count(r => r == existingRole).Should().Be(1);
            user.GetDomainEvents().Should().BeEmpty();
        }

        [Fact]
        public void AddRole_WithMultipleRoles_ShouldMaintainAllRoles()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();

            // Act
            user.AddRole(UserData.Administrator);
            user.AddRole(UserData.Premium);

            // Assert
            user.Roles.Should().HaveCount(3); // Including default Registered role
            user.Roles.Should().Contain(UserData.Registered);
            user.Roles.Should().Contain(UserData.Administrator);
            user.Roles.Should().Contain(UserData.Premium);
        }
    }

    public class ChangeEmailMethod
    {
        [Fact]
        public void ChangeEmail_WithValidEmail_ShouldUpdateEmailAndRaiseEmailChangedEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var newEmail = UserData.NewEmail;

            // Act
            var result = user.ChangeEmail(newEmail);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.Email.Address.Should().Be(newEmail);
            var events = user.GetDomainEvents();
            events.Should().ContainSingle()
                .Which.Should().BeOfType<EmailChangedDomainEvent>()
                .Which.Should().Match<EmailChangedDomainEvent>(e =>
                    e.UserId == user.Id &&
                    e.NewEmail == newEmail);
        }

        [Fact]
        public void ChangeEmail_WithSameEmail_ShouldReturnSuccessWithoutRaisingEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();

            // Act
            var result = user.ChangeEmail(UserData.DefaultEmail);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.Email.Address.Should().Be(UserData.DefaultEmail);
            user.GetDomainEvents().Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)] // Index for ""
        [InlineData(1)] // Index for "invalid-email"
        [InlineData(2)] // Index for null
        [InlineData(3)] // Index for "@example.com"
        [InlineData(4)] // Index for "user@"
        public void ChangeEmail_WithInvalidEmail_ShouldReturnInvalidEmailError(int index)
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            var originalEmail = user.Email;
            user.ClearDomainEvents();

            // Act
            var result = user.ChangeEmail(UserData.InvalidEmails[index]);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UserErrors.InvalidEmail);
            user.Email.Should().Be(originalEmail);
            user.GetDomainEvents().Should().BeEmpty();
        }
    }

    public class SetIdentityIdMethod
    {
        [Fact]
        public void SetIdentityId_WithValidId_ShouldUpdateIdentityId()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            var identityId = UserData.IdentityId;

            // Act
            user.SetIdentityId(identityId);

            // Assert
            user.IdentityId.Should().Be(identityId);
        }

        [Fact]
        public void SetIdentityId_WithMultipleCalls_ShouldUpdateToLatestValue()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            var firstIdentityId = UserData.IdentityId;
            var secondIdentityId = UserData.SecondIdentityId;

            // Act
            user.SetIdentityId(firstIdentityId);
            user.SetIdentityId(secondIdentityId);

            // Assert
            user.IdentityId.Should().Be(secondIdentityId);
        }

        [Theory]
        [InlineData(0)] // Index for ""
        [InlineData(1)] // Index for "   "
        [InlineData(2)] // Index for null
        public void SetIdentityId_WithNullOrEmptyId_ShouldStillSetValue(int index)
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;

            // Act
            user.SetIdentityId(UserData.InvalidNames[index]);

            // Assert
            user.IdentityId.Should().Be(UserData.InvalidNames[index]);
        }
    }
}


// using FluentAssertions;
// using Xunit;
// using Blogify.Domain.Users;
// using Blogify.Domain.Users.Events;
// using Blogify.Domain.Abstractions;
// using Blogify.Domain.UnitTests.Infrastructure;
//
// namespace Blogify.Domain.UnitTests.Users;
//
// public class UserTests : BaseTest
// {
//     [Fact]
//     public void Create_WithValidParameters_ShouldCreateUser()
//     {
//         // Arrange
//         var firstName = new FirstName("John");
//         var lastName = new LastName("Doe");
//         var email = "john.doe@example.com";
//
//         // Act
//         var result = User.Create(firstName, lastName, email);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         var user = result.Value;
//         user.Should().NotBeNull();
//         user.FirstName.Should().Be(firstName);
//         user.LastName.Should().Be(lastName);
//         user.Email.Address.Should().Be(email);
//
//         // Verify domain events
//         var domainEvents = user.GetDomainEvents();
//         domainEvents.Should().HaveCount(2);
//
//         var createdEvent = domainEvents.OfType<UserCreatedDomainEvent>().FirstOrDefault();
//         createdEvent.Should().NotBeNull();
//         createdEvent.UserId.Should().Be(user.Id);
//
//         var roleAssignedEvent = domainEvents.OfType<RoleAssignedDomainEvent>().FirstOrDefault();
//         roleAssignedEvent.Should().NotBeNull();
//         roleAssignedEvent.RoleId.Should().Be(Role.Registered.Id);
//     }
//
//     [Fact]
//     public void Create_WithEmptyFirstName_ShouldReturnFailure()
//     {
//         // Arrange
//         var firstName = new FirstName(string.Empty);
//         var lastName = new LastName("Doe");
//         var email = "john.doe@example.com";
//
//         // Act
//         var result = User.Create(firstName, lastName, email);
//
//         // Assert
//         result.IsFailure.Should().BeTrue();
//         result.Error.Should().Be(UserErrors.InvalidFirstName);
//     }
//
//     [Fact]
//     public void Create_WithNullFirstName_ShouldReturnFailure()
//     {
//         // Arrange
//         var firstName = new FirstName(null!); // Null-forgiving operator
//         var lastName = new LastName("Doe");
//         var email = "john.doe@example.com";
//
//         // Act
//         var result = User.Create(firstName, lastName, email);
//
//         // Assert
//         result.IsFailure.Should().BeTrue();
//         result.Error.Should().Be(UserErrors.InvalidFirstName);
//     }
//
//     [Fact]
//     public void Create_WithEmptyLastName_ShouldReturnFailure()
//     {
//         // Arrange
//         var firstName = new FirstName("John");
//         var lastName = new LastName(string.Empty);
//         var email = "john.doe@example.com";
//
//         // Act
//         var result = User.Create(firstName, lastName, email);
//
//         // Assert
//         result.IsFailure.Should().BeTrue();
//         result.Error.Should().Be(UserErrors.InvalidLastName);
//     }
//
//     [Fact]
//     public void Create_WithNullLastName_ShouldReturnFailure()
//     {
//         // Arrange
//         var firstName = new FirstName("John");
//         var lastName = new LastName(null!); // Null-forgiving operator
//         var email = "john.doe@example.com";
//
//         // Act
//         var result = User.Create(firstName, lastName, email);
//
//         // Assert
//         result.IsFailure.Should().BeTrue();
//         result.Error.Should().Be(UserErrors.InvalidLastName);
//     }
//
//     [Theory]
//     [InlineData("john.doe@example.com", true)]
//     [InlineData(null, false)]
//     [InlineData("", false)]
//     [InlineData("invalid-email-without-at", false)]
//     [InlineData("invalid@", false)]
//     [InlineData("@invalid", false)]
//     public void Email_Create_ShouldValidateEmailFormat(string email, bool expectedIsSuccess)
//     {
//         // Act
//         var result = Email.Create(email);
//
//         // Assert
//         result.IsSuccess.Should().Be(expectedIsSuccess);
//         if (!expectedIsSuccess)
//         {
//             result.Error.Should().BeOneOf(EmailErrors.Empty, EmailErrors.InvalidFormat);
//         }
//     }
//
//     [Fact]
//     public void AddRole_WhenRoleNotPresent_ShouldAddRoleAndRaiseEvent()
//     {
//         // Arrange
//         var user = CreateValidUser();
//         user.ClearDomainEvents();
//         var newRole = new Role(2, "Admin");
//
//         // Act
//         user.AddRole(newRole);
//
//         // Assert
//         user.Roles.Should().Contain(newRole);
//         var domainEvents = user.GetDomainEvents();
//         domainEvents.Should().HaveCount(1);
//
//         var roleAssignedEvent = domainEvents.OfType<RoleAssignedDomainEvent>().FirstOrDefault();
//         roleAssignedEvent.Should().NotBeNull();
//         roleAssignedEvent.RoleId.Should().Be(newRole.Id);
//     }
//
//     [Fact]
//     public void AddRole_WhenRoleAlreadyPresent_ShouldNotAddDuplicate()
//     {
//         // Arrange
//         var user = CreateValidUser();
//         var role = Role.Registered;
//
//         // Act
//         user.AddRole(role);
//
//         // Assert
//         user.Roles.Should().ContainSingle(r => r.Id == role.Id);
//     }
//
//     [Fact]
//     public void ChangeEmail_WithValidEmail_ShouldUpdateEmailAndRaiseEvent()
//     {
//         // Arrange
//         var user = CreateValidUser();
//         var newEmail = "new.email@example.com";
//
//         user.ClearDomainEvents();
//         
//         // Act
//         var result = user.ChangeEmail(newEmail);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         user.Email.Address.Should().Be(newEmail);
//         var domainEvents = user.GetDomainEvents();
//         domainEvents.Should().HaveCount(1);
//
//         var emailChangedEvent = domainEvents.OfType<EmailChangedDomainEvent>().FirstOrDefault();
//         emailChangedEvent.Should().NotBeNull();
//         emailChangedEvent.NewEmail.Should().Be(newEmail);
//     }
//
//     [Fact]
//     public void ChangeEmail_WithInvalidEmail_ShouldReturnFailure()
//     {
//         // Arrange
//         var user = CreateValidUser();
//         var invalidEmail = "invalid-email";
//
//         // Act
//         var result = user.ChangeEmail(invalidEmail);
//
//         // Assert
//         result.IsFailure.Should().BeTrue();
//         result.Error.Should().Be(EmailErrors.InvalidFormat);
//     }
//
//     [Fact]
//     public void ChangeEmail_WithSameEmail_ShouldNotChangeEmailAndNotRaiseEvent()
//     {
//         // Arrange
//         var user = CreateValidUser();
//         var currentEmail = user.Email.Address;
//         user.ClearDomainEvents();
//
//         // Act
//         var result = user.ChangeEmail(currentEmail);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         user.Email.Address.Should().Be(currentEmail);
//         user.GetDomainEvents().Should().BeEmpty();
//     }
//
//     private static User CreateValidUser()
//     {
//         var firstName = new FirstName("John");
//         var lastName = new LastName("Doe");
//         var email = "john.doe@example.com";
//         return User.Create(firstName, lastName, email).Value;
//     }
// }
