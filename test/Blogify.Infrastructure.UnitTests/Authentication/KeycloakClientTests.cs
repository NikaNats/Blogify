using System.Net;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Users;
using Blogify.Infrastructure.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using Shouldly;

namespace Blogify.Infrastructure.UnitTests.Authentication;

public class KeycloakClientTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(respond(request));
    }

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => throw exception;
    }

    [Fact]
    public async Task RegisterAsync_On409_ShouldThrowIdentityAlreadyExists()
    {
        var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict)))
        {
            BaseAddress = new Uri("http://keycloak.test/admin/")
        };
        var service = new AuthenticationService(client, NullLogger<AuthenticationService>.Instance);
        var user = User.Create(
            FirstName.Create("a").Value, LastName.Create("b").Value, Blogify.Domain.Users.Email.Create("a@b.com").Value).Value;

        await Should.ThrowAsync<IdentityAlreadyExistsException>(() => service.RegisterAsync(user, "pw"));
    }

    [Fact]
    public async Task GetAccessTokenAsync_OnResilienceTimeout_ShouldReturnFailureInsteadOfThrowing()
    {
        var client = new HttpClient(new ThrowingHandler(new TimeoutRejectedException("timed out")));
        var options = Options.Create(new KeycloakOptions { TokenUrl = "http://keycloak.test/token" });
        var service = new JwtService(client, options);

        var result = await service.GetAccessTokenAsync("a@b.com", "pw");

        result.IsFailure.ShouldBeTrue();
    }
}
