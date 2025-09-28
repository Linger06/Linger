#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#endif

#if NETFRAMEWORK
using System.Data.SqlClient;
#endif

namespace Linger.Dapper.SqlServer;

public class SqlServerHelper : DapperHelper<SqlConnection>
{
    public SqlServerHelper(string connStr, int? commandTimeout = null) : base(connStr, commandTimeout)
    {
    }

    public SqlServerHelper(SqlConnection dbConnection, int? commandTimeout = null) : base(dbConnection, commandTimeout)
    {
    }

    /// <summary>
    ///     获取SqlServer的实例对象
    /// </summary>
    /// <param name="connStr">符合SqlServer数据库的链接字符串</param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    /// <returns></returns>
    public static SqlServerHelper GetSqlServer(string connStr, int? commandTimeout = null)
    {
        return new SqlServerHelper(connStr, commandTimeout);
    }
}