using Blogify.Domain.Users;
using Blogify.Domain.Users.Events;
using FluentAssertions;

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().NotBe(Guid.Empty);
            result.Value.FirstName.Should().Be(firstName);
            result.Value.LastName.Should().Be(lastName);
            result.Value.Email.Address.Should().Be(email.Address);
            result.Value.IdentityId.Should().BeEmpty();
            result.Value.Roles.Should().ContainSingle(r => r == UserData.Registered);
        }

        [Fact]
        public void Create_WithValidInputs_ShouldRaiseUserCreatedAndRoleAssignedEvents()
        {
            // Act
            var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, UserData.DefaultEmail);

            // Assert
            var events = result.Value.DomainEvents;
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
            // Arrange
            var invalidEmailResult = UserData.InvalidEmails[index];

            // Act
            if (invalidEmailResult.IsSuccess)
            {
                var result = User.Create(UserData.DefaultFirstName, UserData.DefaultLastName, invalidEmailResult.Value);

                // Assert
                result.IsFailure.Should().BeTrue();
                result.Error.Should().Be(UserErrors.InvalidEmail);
            }
            else
            {
                // Assert
                invalidEmailResult.IsFailure.Should().BeTrue();
                invalidEmailResult.Error.Should().Be(UserErrors.InvalidEmail);
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
            firstNameResult.IsFailure.Should().BeTrue();
            firstNameResult.Error.Should().Be(UserErrors.InvalidFirstName);
        }

        [Fact]
        public void Create_WithLongFirstName_ShouldReturnFirstNameTooLongError()
        {
            // Arrange
            var longFirstName = new string('a', 51);

            // Act
            var firstNameResult = FirstName.Create(longFirstName);

            // Assert
            firstNameResult.IsFailure.Should().BeTrue();
            firstNameResult.Error.Should().Be(UserErrors.FirstNameTooLong);
        }

        [Fact]
        public void Create_WithLongLastName_ShouldReturnLastNameTooLongError()
        {
            // Arrange
            var longLastName = new string('a', 51);

            // Act
            var lastNameResult = LastName.Create(longLastName);

            // Assert
            lastNameResult.IsFailure.Should().BeTrue();
            lastNameResult.Error.Should().Be(UserErrors.LastNameTooLong);
        }

        [Fact]
        public void Create_WithLongEmail_ShouldReturnEmailTooLongError()
        {
            // Arrange
            var longEmail = new string('a', 245) + "@example.com"; // 257 characters

            // Act
            var emailResult = Email.Create(longEmail);

            // Assert
            emailResult.IsFailure.Should().BeTrue();
            emailResult.Error.Should().Be(UserErrors.EmailTooLong);
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
            var events = user.DomainEvents;
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
            user.DomainEvents.Should().BeEmpty();
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
            user.Email.Address.Should().Be(newEmail.Address);
            var events = user.DomainEvents;
            events.Should().ContainSingle()
                .Which.Should().BeOfType<EmailChangedDomainEvent>()
                .Which.Should().Match<EmailChangedDomainEvent>(e =>
                    e.UserId == user.Id &&
                    e.NewEmail == newEmail.Address);
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
            user.Email.Address.Should().Be(UserData.DefaultEmail.Address);
            user.DomainEvents.Should().BeEmpty();
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
                result.IsFailure.Should().BeTrue();
                result.Error.Should().Be(UserErrors.InvalidEmail);
                user.Email.Should().Be(originalEmail);
                user.DomainEvents.Should().BeEmpty();
            }
            else
            {
                // Assert
                invalidEmailResult.IsFailure.Should().BeTrue();
                invalidEmailResult.Error.Should().Be(UserErrors.InvalidEmail);
            }
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
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
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
            result.IsSuccess.Should().BeTrue();
            user.FirstName.Value.Should().Be("Jane");
            user.LastName.Value.Should().Be("Smith");
            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserNameChangedDomainEvent>()
                .Which.Should().Match<UserNameChangedDomainEvent>(e =>
                    e.Id == user.Id &&
                    e.OldFirstName == "John" &&
                    e.OldLastName == "Doe" &&
                    e.FirstNameValue == "Jane" &&
                    e.LastNameValue == "Smith");
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
            result.IsSuccess.Should().BeTrue();
            user.FirstName.Should().Be(UserData.DefaultFirstName);
            user.LastName.Should().Be(UserData.DefaultLastName);
            user.DomainEvents.Should().BeEmpty();
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
            result.IsSuccess.Should().BeTrue();
            user.FirstName.Value.Should().Be("Jane");
            user.LastName.Should().Be(UserData.DefaultLastName);
            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserNameChangedDomainEvent>()
                .Which.Should().Match<UserNameChangedDomainEvent>(e =>
                    e.Id == user.Id &&
                    e.OldFirstName == "John" &&
                    e.FirstNameValue == "Jane");
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
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("John");
        }

        [Fact]
        public void Create_WithEmptyName_ShouldFail()
        {
            // Act
            var result = FirstName.Create("");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UserErrors.InvalidFirstName);
        }

        [Fact]
        public void Create_WithLongName_ShouldFail()
        {
            // Arrange
            var longName = new string('a', 51);

            // Act
            var result = FirstName.Create(longName);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UserErrors.FirstNameTooLong);
        }

        [Fact]
        public void Create_WithMaxLengthName_ShouldSucceed()
        {
            // Arrange
            var maxName = new string('a', 50);

            // Act
            var result = FirstName.Create(maxName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(maxName);
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
            result.IsSuccess.Should().BeTrue();
            result.Value.Address.Should().Be("test@example.com");
        }

        [Fact]
        public void Create_WithLongEmail_ShouldFail()
        {
            // Arrange
            var longEmail = new string('a', 245) + "@example.com";

            // Act
            var result = Email.Create(longEmail);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(UserErrors.EmailTooLong);
        }

        [Fact]
        public void Create_WithMaxLengthEmail_ShouldSucceed()
        {
            // Arrange
            var maxEmail = new string('a', 64) + "@" + new string('b', 185) + ".com"; // Total 254

            // Act
            var result = Email.Create(maxEmail);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Address.Should().Be(maxEmail.ToLowerInvariant());
        }

        [Fact]
        public void Emails_WithDifferentCases_ShouldBeEqual()
        {
            // Arrange
            var email1 = Email.Create("Test@Example.com").Value;
            var email2 = Email.Create("test@EXAMPLE.COM").Value;

            // Assert
            email1.Should().Be(email2);
        }
    }
}