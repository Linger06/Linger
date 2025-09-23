using System.Data.SQLite;

namespace Linger.Dapper.Sqlite;

public class SqliteHelper : DapperHelper<SQLiteConnection>
{
    public SqliteHelper(string connStr, int? commandTimeout = null) : base(connStr, commandTimeout)
    {
    }

    public SqliteHelper(SQLiteConnection dbConnection, int? commandTimeout = null) : base(dbConnection, commandTimeout)
    {
    }

    /// <summary>
    ///     获取Sqlite的实例对象
    /// </summary>
    /// <param name="connStr">符合Sqlite数据库的链接字符串</param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    /// <returns></returns>
    public static SqliteHelper GetSqlite(string connStr, int? commandTimeout = null)
    {
        var dbConnection = new SQLiteConnection(connStr);

        return new SqliteHelper(dbConnection, commandTimeout);
    }
}
