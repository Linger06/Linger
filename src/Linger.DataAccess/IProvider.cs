using System.Data.Common;

namespace Linger.DataAccess;

public interface IProvider
{
    DbConnection CreateConnection();

    DbConnection CreateConnection(string connectionString);

    DbCommand CreateCommand();
    DbDataAdapter CreateDataAdapter();

    DbDataAdapter CreateDataAdapter(DbCommand dbCommand);
    //DbTransaction CreateTransaction();
}