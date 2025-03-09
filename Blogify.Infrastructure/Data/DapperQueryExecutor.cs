using System.Data;
using Dapper;
using Blogify.Application.Abstractions.Data;

namespace Blogify.Infrastructure.Data
{
    public class DapperQueryExecutor : IDapperQueryExecutor
    {
        public Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object parameters)
        {
            return connection.QuerySingleAsync<T>(sql, parameters);
        }
    }
}