using System.Data;
using Linger.Extensions.Core;
using Microsoft.Data.SqlClient;

namespace Linger.DataAccess.SqlServer;

/// <summary>
/// SQL Server 数据库帮助类，提供 SQL Server 特有的功能
/// </summary>
/// <param name="strConnection">数据库连接字符串</param>
public class SqlServerHelper(string connectionString) : Database(new SqlServerProvider(), connectionString)
{
    /// <summary>
    /// 执行 SQL 查询并返回 DataTable（与 Oracle/SqliteHelper 风格一致）
    /// </summary>
    /// <param name="sql">SQL 查询语句</param>
    /// <param name="parameters">可选参数</param>
    /// <returns>查询结果 DataTable</returns>
    /// <exception cref="ArgumentException">当 sql 为空时抛出</exception>
    /// <example>
    /// <code>
    /// var dt = helper.Query("SELECT * FROM Users WHERE Id = @Id", new SqlParameter("@Id", 1));
    /// </code>
    /// </example>
    public DataTable Query(string sql, params SqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));
        return QueryTable(sql, parameters);
    }

    /// <summary>
    /// 异步执行 SQL 查询并返回 DataTable（与 Oracle/SqliteHelper 风格一致）
    /// </summary>
    /// <param name="sql">SQL 查询语句</param>
    /// <param name="parameters">可选参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果 DataTable</returns>
    /// <exception cref="ArgumentException">当 sql 为空时抛出</exception>
    /// <example>
    /// <code>
    /// var dt = await helper.QueryAsync("SELECT * FROM Users WHERE Name = @Name", new SqlParameter("@Name", "张三"));
    /// </code>
    /// </example>
    public Task<DataTable> QueryAsync(string sql, SqlParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));
        return QueryTableAsync(sql, parameters, cancellationToken);
    }

    /// <summary>
    /// 海量数据插入方法
    /// (调用该方法需要注意，DataTable中的字段名称必须和数据库中的字段名称一一对应)
    /// </summary>
    /// <param name="table">内存表数据</param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认为 1000</param>
    /// <param name="timeout">超时时间（秒），默认为 100</param>
    /// <exception cref="ArgumentNullException">当 table 或 tableName 为空时抛出</exception>
    /// <exception cref="ArgumentException">当 table 没有数据行时抛出</exception>
    public void AddByBulkCopy(DataTable table, string tableName, int batchSize = 1000, int timeout = 100)
    {
        ArgumentNullException.ThrowIfNull(table, nameof(table));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        if (table.Rows.Count == 0)
        {
            return;
        }

        using var bulk = new SqlBulkCopy(ConnString)
        {
            BatchSize = batchSize,
            BulkCopyTimeout = timeout,
            DestinationTableName = tableName
        };

        bulk.WriteToServer(table);
    }

    /// <summary>
    /// 海量数据插入方法（异步版本）
    /// </summary>
    /// <param name="table">内存表数据</param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认为 1000</param>
    /// <param name="timeout">超时时间（秒），默认为 100</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    /// <exception cref="ArgumentNullException">当 table 或 tableName 为空时抛出</exception>
    /// <exception cref="ArgumentException">当 table 没有数据行时抛出</exception>
    public async Task AddByBulkCopyAsync(DataTable table, string tableName, int batchSize = 1000, int timeout = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table, nameof(table));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        if (table.Rows.Count == 0)
        {
            return;
        }

        using var bulk = new SqlBulkCopy(ConnString)
        {
            BatchSize = batchSize,
            BulkCopyTimeout = timeout,
            DestinationTableName = tableName
        };

        await bulk.WriteToServerAsync(table, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 获取指定字段的最大值并加1，通常用于生成下一个ID
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名称</param>
    /// <returns>最大值加1，如果没有数据则返回1，如果字段不是数值类型则返回null</returns>
    /// <exception cref="ArgumentException">当 fieldName 或 tableName 为空时抛出</exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛出</exception>
    public int? GetMaxId(string fieldName, string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        try
        {
            var sql = $"SELECT MAX({fieldName}) + 1 FROM {tableName}";
            var obj = FindMaxBySql(sql);

            return obj switch
            {
                null => 1,
                _ when obj.IsInt() => obj.ToIntOrDefault(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取表 {tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 获取指定字段的最大值并加1（异步版本）
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最大值加1，如果没有数据则返回1，如果字段不是数值类型则返回null</returns>
    /// <exception cref="ArgumentException">当 fieldName 或 tableName 为空时抛出</exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛出</exception>
    public async Task<int?> GetMaxIdAsync(string fieldName, string tableName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        try
        {
            var sql = $"SELECT MAX({fieldName}) + 1 FROM {tableName}";
            var obj = await ExecuteScalarAsync(CommandType.Text, sql).ConfigureAwait(false);

            return obj switch
            {
                null => 1,
                _ when obj.IsInt() => obj.ToIntOrDefault(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取表 {tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 检查指定SQL查询是否返回数据
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <returns>如果有数据返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当 sql 为空时抛出</exception>
    /// <example>
    /// <code>
    /// // 检查用户是否存在
    /// var userExists = helper.Exists("SELECT COUNT(*) FROM Users WHERE Id = 1");
    /// 
    /// // 检查表中是否有数据
    /// var hasData = helper.Exists("SELECT COUNT(*) FROM Products WHERE Price > 100");
    /// 
    /// // 检查特定条件的记录是否存在
    /// var hasActiveUsers = helper.Exists("SELECT COUNT(*) FROM Users WHERE Status = 'Active' AND LastLogin > '2024-01-01'");
    /// </code>
    /// </example>
    public bool Exists(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));

        var count = FindCountBySql(sql);
        return count > 0;
    }

    /// <summary>
    /// 检查指定SQL查询是否返回数据（异步版本）
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>如果有数据返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当 sql 为空时抛出</exception>
    /// <example>
    /// <code>
    /// // 异步检查用户是否存在
    /// var userExists = await helper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Email = 'user@example.com'");
    /// 
    /// // 异步检查订单是否存在
    /// var orderExists = await helper.ExistsAsync("SELECT COUNT(*) FROM Orders WHERE OrderDate >= DATEADD(day, -30, GETDATE())");
    /// 
    /// // 使用取消令牌的异步检查
    /// using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    /// var hasExpiredSessions = await helper.ExistsAsync("SELECT COUNT(*) FROM UserSessions WHERE ExpiryDate < GETDATE()", cts.Token);
    /// </code>
    /// </example>
    public async Task<bool> ExistsAsync(string sql, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));

        var count = await FindCountBySqlAsync(sql).ConfigureAwait(false);
        return count > 0;
    }

    /// <summary>
    /// 使用 SQL Server 特有的 BulkCopy 进行批量插入
    /// </summary>
    /// <param name="dt">数据表</param>
    /// <returns>始终返回 true，表示成功</returns>
    public new bool BulkInsert(DataTable dt)
    {
        if (dt?.Rows.Count > 0)
        {
            AddByBulkCopy(dt, dt.TableName);
            return true;
        }
        return false;
    }

    /// <summary>
    ///     执行SQL语句，返回影响的记录数
    /// </summary>
    /// <param name="sqlString">SQL语句</param>
    /// <returns>影响的记录数</returns>
    public int ExecuteSql(string sqlString)
    {
        using var connection = new SqlConnection(ConnString);
        using var cmd = new SqlCommand(sqlString, connection);
        try
        {
            connection.Open();
            var rows = cmd.ExecuteNonQuery();
            return rows;
        }
        catch (SqlException)
        {
            connection.Close();
            throw;
        }
    }

    /// <summary>
    ///     执行一条计算查询结果语句，返回查询结果（object）。
    /// </summary>
    /// <param name="sqlString">计算查询结果语句</param>
    /// <returns>查询结果（object）</returns>
    public object? GetSingle(string sqlString)
    {
        using var connection = new SqlConnection(ConnString);
        using var cmd = new SqlCommand(sqlString, connection);
        try
        {
            connection.Open();
            var obj = cmd.ExecuteScalar();
            if (obj.IsNullOrDbNull())
            {
                return null;
            }

            return obj;
        }
        catch (SqlException)
        {
            connection.Close();
            throw;
        }
    }

    /// <summary>
    ///     执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
    /// </summary>
    /// <param name="strSql">查询语句</param>
    /// <returns>SqlDataReader</returns>
    public SqlDataReader ExecuteReader(string strSql)
    {
        var connection = new SqlConnection(ConnString);
        var cmd = new SqlCommand(strSql, connection);
        connection.Open();
        SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        return myReader;
    }

    /// <summary>
    ///     执行查询语句，返回DataSet
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="times">超时时间(秒)</param>
    /// <returns>DataSet</returns>
    public DataSet Query(string sqlString, int? times = null)
    {
        using var connection = new SqlConnection(ConnString);
        var ds = new DataSet();
        try
        {
            connection.Open();
            var command = new SqlDataAdapter(sqlString, connection);
            if (times != null)
            {
                command.SelectCommand.CommandTimeout = (int)times;
            }

            _ = command.Fill(ds, "ds");
        }
        catch
        {
            throw;
        }

        return ds;
    }
}
