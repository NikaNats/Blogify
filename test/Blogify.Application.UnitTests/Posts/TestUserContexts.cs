using Blogify.Application.Abstractions.Authentication;
using NSubstitute;

namespace Blogify.Application.UnitTests.Posts;

internal static class TestUserContexts
{
    public static IUserContext Administrator()
    {
        var context = Substitute.For<IUserContext>();
        context.IsInRole("Administrator").Returns(true);
        return context;
    }

    public static IUserContext User(Guid userId)
    {
        var context = Substitute.For<IUserContext>();
        context.UserId.Returns(userId);
        return context;
    }
}
