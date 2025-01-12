using Blogify.Api.Controllers.Users;

namespace Blogify.FunctionalTests.Users;

internal static class UserData
{
    public static RegisterUserRequest RegisterTestUserRequest = new("test@test.com", "test", "test", "12345");
}