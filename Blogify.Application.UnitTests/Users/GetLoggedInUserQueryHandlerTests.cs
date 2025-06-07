using System.Data;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Users.GetLoggedInUser;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using NSubstitute;

namespace Blogify.Application.UnitTests.Users;

public class GetLoggedInUserQueryHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly IDbConnection _dbConnection;
    private readonly GetLoggedInUserQueryHandler _handler;
    private readonly IDapperQueryExecutor _queryExecutor;

    public GetLoggedInUserQueryHandlerTests()
    {
        var sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        var userContext = Substitute.For<IUserContext>();
        _dbConnection = Substitute.For<IDbConnection>();
        _queryExecutor = Substitute.For<IDapperQueryExecutor>();

        sqlConnectionFactory.CreateConnection().Returns(_dbConnection);
        userContext.IdentityId.Returns("test-identity-id");

        _handler = new GetLoggedInUserQueryHandler(sqlConnectionFactory, userContext, _queryExecutor);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUserResponse()
    {
        // Arrange
        var expectedUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _queryExecutor
            .QuerySingleAsync<UserResponse>(_dbConnection, Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(expectedUser));

        var query = new GetLoggedInUserQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUser, result.Value);
    }

    [Fact]
    public async Task Handle_InvalidOperationException_ReturnsFailureWithUserNotFoundError()
    {
        // Arrange
        _queryExecutor
            .QuerySingleAsync<UserResponse>(_dbConnection, Arg.Any<string>(), Arg.Any<object>())
            .Returns<Task<UserResponse>>(_ => throw new InvalidOperationException());

        var query = new GetLoggedInUserQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_GenericException_ReturnsFailureWithDatabaseError()
    {
        // Arrange
        var exceptionMessage = "Test exception";
        _queryExecutor
            .QuerySingleAsync<UserResponse>(_dbConnection, Arg.Any<string>(), Arg.Any<object>())
            .Returns<Task<UserResponse>>(_ => throw new Exception(exceptionMessage));

        var query = new GetLoggedInUserQuery();

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Database.Error", result.Error.Code);
        Assert.Contains(exceptionMessage, result.Error.Description);
        Assert.Equal(ErrorType.Unexpected, result.Error.Type);
    }
}