using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Users.LogInUser;

public sealed record LogInUserCommand(string Email, string Password)
    : ICommand<AccessTokenResponse>;