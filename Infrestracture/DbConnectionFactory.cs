using Microsoft.Data.SqlClient;

namespace Infrastructure;

public class DbConnectionFactory(string connectionString)
{
    public SqlConnection CreateConnection() => new SqlConnection(connectionString);
}