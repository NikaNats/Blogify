using System.Net.Http.Json;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Users;
using Blogify.Infrastructure.Authentication.Models;
using Microsoft.Extensions.Logging;

namespace Blogify.Infrastructure.Authentication;

internal sealed class AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger)
    : IAuthenticationService
{
    private const string PasswordCredentialType = "password";

    public async Task<string> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userRepresentationModel = UserRepresentationModel.FromUser(user);

        userRepresentationModel.Credentials =
        [
            new CredentialRepresentationModel
            {
                Value = password,
                Temporary = false,
                Type = PasswordCredentialType
            }
        ];

        var response = await httpClient.PostAsJsonAsync(
            "users",
            userRepresentationModel,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Registration failed with status code {StatusCode}. Response: {ErrorContent}",
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Failed to register user. Status code: {response.StatusCode}");
        }

        return ExtractIdentityIdFromLocationHeader(response);
    }

    public async Task DeleteIdentityAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"users/{identityId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully deleted identity {IdentityId}", identityId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Failed to delete identity {IdentityId}. Status code: {StatusCode}, Content: {ErrorContent}",
                    identityId,
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Failed to delete identity {identityId}. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Error occurred while deleting identity {IdentityId}", identityId);
            throw;
        }
    }

    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        var locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null) throw new InvalidOperationException("Location header can't be null");

        var userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        var userIdentityId = locationHeader.Substring(
            userSegmentValueIndex + usersSegmentName.Length);

        return userIdentityId;
    }
}