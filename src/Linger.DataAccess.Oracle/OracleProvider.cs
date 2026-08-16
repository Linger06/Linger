using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

namespace Linger.DataAccess.Oracle;

[Obsolete("OracleProvider will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
public class OracleProvider : IProvider
{
    public DbCommand CreateCommand()
    {
        return new OracleCommand();
    }

    public DbConnection CreateConnection()
    {
        return new OracleConnection();
    }

    public DbConnection CreateConnection(string connectionString)
    {
        return new OracleConnection(connectionString);
    }

    public DbDataAdapter CreateDataAdapter()
    {
        return new OracleDataAdapter();
    }

    public DbDataAdapter CreateDataAdapter(DbCommand dbCommand)
    {
        return new OracleDataAdapter((OracleCommand)dbCommand);
    }
}
