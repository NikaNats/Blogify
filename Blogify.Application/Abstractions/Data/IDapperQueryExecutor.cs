using System.Data;

namespace Blogify.Application.Abstractions.Data
{
    public interface IDapperQueryExecutor
    {
        Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object parameters);
    }
}
