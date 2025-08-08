using Blogify.Domain.Users;
using Blogify.Domain.Users.Events;
using Shouldly;

// Changed namespace

namespace Blogify.Domain.UnitTests.Users;

public static class UserTests
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
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldNotBe(Guid.Empty);
            result.Value.FirstName.ShouldBe(firstName);
            result.Value.LastName.ShouldBe(lastName);
            result.Value.Email.Address.ShouldBe(email.Address);
            result.Value.IdentityId.ShouldBeNull();
            result.Value.Status.ShouldBe(UserStatus.Pending);
            result.Value.Roles.ShouldContain(r => r == UserData.Registered, 1);
        }

        [Fact]
        public void Create_WithValidInputs_ShouldRaiseUserCreatedAndRoleAssignedEvents()
        {
            // Act
            var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail);

            // Assert
            var events = result.Value.DomainEvents;
            events.Count.ShouldBe(2);

            // Find and validate UserCreatedDomainEvent
            var userCreatedEvent = events.OfType<UserCreatedDomainEvent>().FirstOrDefault();
            userCreatedEvent.ShouldNotBeNull();
            userCreatedEvent.UserId.ShouldBe(result.Value.Id);

            // Find and validate RoleAssignedDomainEvent
            var roleAssignedEvent = events.OfType<RoleAssignedDomainEvent>().FirstOrDefault();
            roleAssignedEvent.ShouldNotBeNull();
            roleAssignedEvent.UserId.ShouldBe(result.Value.Id);
            roleAssignedEvent.RoleId.ShouldBe(UserData.Registered.Id);
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
            lastNameResult.IsFailure.ShouldBeTrue();
            lastNameResult.Error.ShouldBe(UserErrors.InvalidLastName);
        }

        [Theory]
        [InlineData(0)] // Index for ""
        [InlineData(1)] // Index for "invalid-email"
        [InlineData(2)] // Index for null
        [InlineData(3)] // Index for "@example.com"
        [InlineData(4)] // Index for "user@"
        public void Create_WithInvalidEmail_ShouldReturnInvalidEmailError(int index)
        {
            // Arrange
            var invalidEmailResult = UserData.InvalidEmails[index];

            // Act
            if (invalidEmailResult.IsSuccess)
            {
                var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, invalidEmailResult.Value);

                // Assert
                result.IsFailure.ShouldBeTrue();
                result.Error.ShouldBe(UserErrors.InvalidEmail);
            }
            else
            {
                // Assert
                invalidEmailResult.IsFailure.ShouldBeTrue();
                invalidEmailResult.Error.ShouldBe(UserErrors.InvalidEmail);
            }
        }

        [Theory]
        [InlineData(0)] // ""
        [InlineData(1)] // null
        [InlineData(2)] // "   "
        public void Create_WithInvalidFirstName_ShouldReturnInvalidFirstNameError(int index)
        {
            // Arrange
            var invalidFirstNameInput = UserData.InvalidNames[index];

            // Act
            var firstNameResult = FirstName.Create(invalidFirstNameInput);

            // Assert
            firstNameResult.IsFailure.ShouldBeTrue();
            firstNameResult.Error.ShouldBe(UserErrors.InvalidFirstName);
        }

        [Fact]
        public void Create_WithLongFirstName_ShouldReturnFirstNameTooLongError()
        {
            // Arrange
            var longFirstName = new string('a', 51);

            // Act
            var firstNameResult = FirstName.Create(longFirstName);

            // Assert
            firstNameResult.IsFailure.ShouldBeTrue();
            firstNameResult.Error.ShouldBe(UserErrors.FirstNameTooLong);
        }

        [Fact]
        public void Create_WithLongLastName_ShouldReturnLastNameTooLongError()
        {
            // Arrange
            var longLastName = new string('a', 51);

            // Act
            var lastNameResult = LastName.Create(longLastName);

            // Assert
            lastNameResult.IsFailure.ShouldBeTrue();
            lastNameResult.Error.ShouldBe(UserErrors.LastNameTooLong);
        }

        [Fact]
        public void Create_WithLongEmail_ShouldReturnEmailTooLongError()
        {
            // Arrange
            var longEmail = new string('a', 245) + "@example.com"; // 257 characters

            // Act
            var emailResult = Email.Create(longEmail);

            // Assert
            emailResult.IsFailure.ShouldBeTrue();
            emailResult.Error.ShouldBe(UserErrors.EmailTooLong);
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
            user.Roles.ShouldContain(newRole);
            var events = user.DomainEvents;
            events.Count.ShouldBe(1);
            var roleEvent = events[0].ShouldBeOfType<RoleAssignedDomainEvent>();
            roleEvent.UserId.ShouldBe(user.Id);
            roleEvent.RoleId.ShouldBe(newRole.Id);
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
            user.Roles.Count(r => r == existingRole).ShouldBe(1);
            user.DomainEvents.ShouldBeEmpty();
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
            user.Roles.Count.ShouldBe(3); // Including default Registered role
            user.Roles.ShouldContain(UserData.Registered);
            user.Roles.ShouldContain(UserData.Administrator);
            user.Roles.ShouldContain(UserData.Premium);
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
            result.IsSuccess.ShouldBeTrue();
            user.Email.Address.ShouldBe(newEmail.Address);
            user.DomainEvents.Count.ShouldBe(1);
            var emailEvent = user.DomainEvents[0].ShouldBeOfType<EmailChangedDomainEvent>();
            emailEvent.UserId.ShouldBe(user.Id);
            emailEvent.NewEmail.ShouldBe(newEmail.Address);
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
            result.IsSuccess.ShouldBeTrue();
            user.Email.Address.ShouldBe(UserData.DefaultEmail.Address);
            user.DomainEvents.ShouldBeEmpty();
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
            var invalidEmailResult = UserData.InvalidEmails[index];

            // Act
            if (invalidEmailResult.IsSuccess)
            {
                var result = user.ChangeEmail(invalidEmailResult.Value);

                // Assert
                result.IsFailure.ShouldBeTrue();
                result.Error.ShouldBe(UserErrors.InvalidEmail);
                user.Email.ShouldBe(originalEmail);
                user.DomainEvents.ShouldBeEmpty();
            }
            else
            {
                // Assert
                invalidEmailResult.IsFailure.ShouldBeTrue();
                invalidEmailResult.Error.ShouldBe(UserErrors.InvalidEmail);
            }
        }
    }

    public class ActivateMethod
    {
        [Fact]
        public void Activate_FromPending_ShouldSetIdentityIdStatusActiveAndRaiseEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var identityId = UserData.IdentityId;

            // Act
            user.Activate(identityId);

            // Assert
            user.IdentityId.ShouldBe(identityId);
            user.Status.ShouldBe(UserStatus.Active);
            user.DomainEvents.Count.ShouldBe(1);
            user.DomainEvents[0].ShouldBeOfType<UserActivatedDomainEvent>();
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldNotRaiseDuplicateEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.Activate(UserData.IdentityId);
            user.ClearDomainEvents();

            // Act
            user.Activate("another-id");

            // Assert
            user.IdentityId.ShouldBe(UserData.IdentityId); // unchanged
            user.Status.ShouldBe(UserStatus.Active);
            user.DomainEvents.ShouldBeEmpty();
        }

        [Fact]
        public void Activate_WhenAlreadyActiveWithoutIdentity_ShouldFillIdentityWithoutEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            // Force active state without identity (edge case simulation)
            typeof(User).GetProperty("Status")!.SetValue(user, UserStatus.Active);
            user.ClearDomainEvents();

            // Act
            user.Activate(UserData.IdentityId);

            // Assert
            user.IdentityId.ShouldBe(UserData.IdentityId);
            user.Status.ShouldBe(UserStatus.Active);
            user.DomainEvents.ShouldBeEmpty();
        }
    }

    public class ChangeNameMethod
    {
        [Fact]
        public void ChangeName_WithValidNewNames_ShouldUpdateNamesAndRaiseEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var newFirstName = FirstName.Create("Jane").Value;
            var newLastName = LastName.Create("Smith").Value;

            // Act
            var result = user.ChangeName(newFirstName, newLastName);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            user.FirstName.Value.ShouldBe("Jane");
            user.LastName.Value.ShouldBe("Smith");
            user.DomainEvents.Count.ShouldBe(1);
            var nameEvent = user.DomainEvents[0].ShouldBeOfType<UserNameChangedDomainEvent>();
            nameEvent.Id.ShouldBe(user.Id);
            nameEvent.OldFirstName.ShouldBe("John");
            nameEvent.OldLastName.ShouldBe("Doe");
            nameEvent.FirstNameValue.ShouldBe("Jane");
            nameEvent.LastNameValue.ShouldBe("Smith");
        }

        [Fact]
        public void ChangeName_WithSameNames_ShouldNotUpdateOrRaiseEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();

            // Act
            var result = user.ChangeName(UserData.DefaultFirstName, UserData.DefaultLastName);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            user.FirstName.ShouldBe(UserData.DefaultFirstName);
            user.LastName.ShouldBe(UserData.DefaultLastName);
            user.DomainEvents.ShouldBeEmpty();
        }

        [Fact]
        public void ChangeName_WithOnlyFirstNameChanged_ShouldUpdateAndRaiseEvent()
        {
            // Arrange
            var user = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail).Value;
            user.ClearDomainEvents();
            var newFirstName = FirstName.Create("Jane").Value;

            // Act
            var result = user.ChangeName(newFirstName, UserData.DefaultLastName);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            user.FirstName.Value.ShouldBe("Jane");
            user.LastName.ShouldBe(UserData.DefaultLastName);
            user.DomainEvents.Count.ShouldBe(1);
            var nameEvent = user.DomainEvents[0].ShouldBeOfType<UserNameChangedDomainEvent>();
            nameEvent.Id.ShouldBe(user.Id);
            nameEvent.OldFirstName.ShouldBe("John");
            nameEvent.FirstNameValue.ShouldBe("Jane");
        }
    }

    public class FirstNameTests
    {
        [Fact]
        public void Create_WithValidName_ShouldSucceedAndTrim()
        {
            // Act
            var result = FirstName.Create(" John ");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe("John");
        }

        [Fact]
        public void Create_WithEmptyName_ShouldFail()
        {
            // Act
            var result = FirstName.Create("");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(UserErrors.InvalidFirstName);
        }

        [Fact]
        public void Create_WithLongName_ShouldFail()
        {
            // Arrange
            var longName = new string('a', 51);

            // Act
            var result = FirstName.Create(longName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(UserErrors.FirstNameTooLong);
        }

        [Fact]
        public void Create_WithMaxLengthName_ShouldSucceed()
        {
            // Arrange
            var maxName = new string('a', 50);

            // Act
            var result = FirstName.Create(maxName);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe(maxName);
        }
    }

    public class EmailTests
    {
        [Fact]
        public void Create_WithValidEmail_ShouldSucceedAndLowercase()
        {
            // Act
            var result = Email.Create("Test@Example.com");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Address.ShouldBe("test@example.com");
        }

        [Fact]
        public void Create_WithLongEmail_ShouldFail()
        {
            // Arrange
            var longEmail = new string('a', 245) + "@example.com";

            // Act
            var result = Email.Create(longEmail);

            // Assert
            var expected = UserErrors.EmailTooLong;
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(expected);
        }

        [Fact]
        public void Create_WithMaxLengthEmail_ShouldSucceed()
        {
            // Arrange
            var maxEmail = new string('a', 64) + "@" + new string('b', 185) + ".com"; // Total 254

            // Act
            var result = Email.Create(maxEmail);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Address.ShouldBe(maxEmail.ToLowerInvariant());
        }

        [Fact]
        public void Emails_WithDifferentCases_ShouldBeEqual()
        {
            // Arrange
            var email1 = Email.Create("Test@Example.com").Value;
            var email2 = Email.Create("test@EXAMPLE.COM").Value;

            // Assert
            email1.ShouldBe(email2);
        }
    }
}