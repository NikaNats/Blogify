using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Dapper;

namespace Blogify.Application.Users.GetLoggedInUser;

internal sealed class GetLoggedInUserQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext,
    IDapperQueryExecutor queryExecutor)
    : IQueryHandler<GetLoggedInUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetLoggedInUserQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var connection = sqlConnectionFactory.CreateConnection();

            const string sql = """
                               SELECT
                                   id AS Id,
                                   first_name AS FirstName,
                                   last_name AS LastName,
                                   email AS Email
                               FROM users
                               WHERE identity_id = @IdentityId
                               """;

            var user = await queryExecutor.QuerySingleAsync<UserResponse>(
                connection,
                sql,
                new { userContext.IdentityId });
            
            return user;
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserResponse>(new Error(
                "Database.Error",
                ex.Message,ErrorType.Unexpected));
        }
    }
}