using System.Data;
using Blogify.Application.Abstractions.Data;
using Npgsql;

namespace Blogify.Infrastructure.Data;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        return connection;
    }
}