using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Users.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;