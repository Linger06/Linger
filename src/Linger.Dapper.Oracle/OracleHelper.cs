using Oracle.ManagedDataAccess.Client;

namespace Linger.Dapper.Oracle;

public class OracleHelper(OracleConnection dbConnection, int? commandTimeout = null)
    : DapperHelper<OracleConnection>(dbConnection, commandTimeout)
{
    /// <summary>
    ///     获取Oracle的实例对象
    /// </summary>
    /// <param name="connStr">符合Oracle数据库的链接字符串</param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    /// <returns></returns>
    public static OracleHelper GetOracle(string connStr, int? commandTimeout = null)
    {
        var dbConnection = new OracleConnection(connStr);

        return new OracleHelper(dbConnection, commandTimeout);
    }
}