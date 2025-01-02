using Asp.Versioning;
using Blogify.Application.Users.GetLoggedInUser;
using Blogify.Application.Users.LogInUser;
using Blogify.Application.Users.RegisterUser;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Users;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
    {
        var query = new GetLoggedInUserQuery();

        Result<UserResponse> result = await sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LogIn(
        LogInUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogInUserCommand(request.Email, request.Password);

        Result<AccessTokenResponse> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return Unauthorized(result.Error);

        return Ok(result.Value);
    }
}