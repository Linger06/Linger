using System.Data.Common;

namespace Linger.DataAccess;

[Obsolete("IProvider will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
public interface IProvider
{
    DbConnection CreateConnection();

    DbConnection CreateConnection(string connectionString);

    DbCommand CreateCommand();
    DbDataAdapter CreateDataAdapter();

    DbDataAdapter CreateDataAdapter(DbCommand dbCommand);
    //DbTransaction CreateTransaction();
}
