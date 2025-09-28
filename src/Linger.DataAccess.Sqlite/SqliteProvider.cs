using System.Data.Common;
using System.Data.SQLite;

namespace Linger.DataAccess.Sqlite;

public class SqliteProvider : IProvider
{
    public DbCommand CreateCommand()
    {
        return new SQLiteCommand();
    }

    public DbConnection CreateConnection()
    {
        return new SQLiteConnection();
    }

    public DbConnection CreateConnection(string connectionString)
    {
        return new SQLiteConnection(connectionString);
    }

    public DbDataAdapter CreateDataAdapter()
    {
        return new SQLiteDataAdapter();
    }

    public DbDataAdapter CreateDataAdapter(DbCommand dbCommand)
    {
        return new SQLiteDataAdapter((SQLiteCommand)dbCommand);
    }
}