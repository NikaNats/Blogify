using System.Net;
using Blogify.Api.Middleware;
using Blogify.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Shouldly;

namespace Blogify.FunctionalTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<int> StatusFor(Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        return context.Response.StatusCode;
    }

    [Fact]
    public async Task UniqueViolation_ShouldReturnConflict()
    {
        var postgres = new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);

        (await StatusFor(new DbUpdateException("save failed", postgres))).ShouldBe((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task OtherDatabaseError_ShouldReturnServerError()
    {
        var postgres = new PostgresException("fk", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation);

        (await StatusFor(new DbUpdateException("save failed", postgres)))
            .ShouldBe((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ConcurrencyException_ShouldReturnConflict()
    {
        var exception = new ConcurrencyException("stale", new DbUpdateConcurrencyException());

        (await StatusFor(exception)).ShouldBe((int)HttpStatusCode.Conflict);
    }
}
