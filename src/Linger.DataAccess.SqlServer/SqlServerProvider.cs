using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Linger.DataAccess.SqlServer;

[Obsolete("SqlServerProvider will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
public class SqlServerProvider : IProvider
{
    public DbCommand CreateCommand()
    {
        return new SqlCommand();
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection();
    }

    public DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public DbDataAdapter CreateDataAdapter()
    {
        return new SqlDataAdapter();
    }

    public DbDataAdapter CreateDataAdapter(DbCommand dbCommand)
    {
        return new SqlDataAdapter((SqlCommand)dbCommand);
    }
}
