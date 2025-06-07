using System.Data;
using Blogify.Application.Abstractions.Data;
using Dapper;

namespace Blogify.Infrastructure.Data;

public class DapperQueryExecutor : IDapperQueryExecutor
{
    public Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object parameters)
    {
        return connection.QuerySingleAsync<T>(sql, parameters);
    }
}