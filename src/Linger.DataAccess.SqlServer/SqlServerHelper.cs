using System.Data;
using Linger.Extensions.Core;
using Microsoft.Data.SqlClient;

namespace Linger.DataAccess.SqlServer;

/// <summary>
/// SQL Server 数据库帮助类，提�?SQL Server 特有的功�?
/// </summary>
/// <param name="strConnection">数据库连接字符串</param>
public class SqlServerHelper(string connectionString) : Database(new SqlServerProvider(), connectionString)
{
    /// <summary>
    /// 海量数据插入方法
    /// (调用该方法需要注意，DataTable中的字段名称必须和数据库中的字段名称一一对应)
    /// </summary>
    /// <param name="table">内存表数�?/param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认�?1000</param>
    /// <param name="timeout">超时时间（秒），默认�?100</param>
    /// <exception cref="ArgumentNullException">�?table �?tableName 为空时抛�?/exception>
    /// <exception cref="ArgumentException">�?table 没有数据行时抛出</exception>
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
    /// <param name="table">内存表数�?/param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认�?1000</param>
    /// <param name="timeout">超时时间（秒），默认�?100</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    /// <exception cref="ArgumentNullException">�?table �?tableName 为空时抛�?/exception>
    /// <exception cref="ArgumentException">�?table 没有数据行时抛出</exception>
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
    /// 获取指定字段的最大值并�?，通常用于生成下一个ID
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名�?/param>
    /// <returns>最大值加1，如果没有数据则返回1，如果字段不是数值类型则返回null</returns>
    /// <exception cref="ArgumentException">�?fieldName �?tableName 为空时抛�?/exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛�?/exception>
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
            throw new InvalidOperationException($"获取�?{tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 获取指定字段的最大值并�?（异步版本）
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名�?/param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最大值加1，如果没有数据则返回1，如果字段不是数值类型则返回null</returns>
    /// <exception cref="ArgumentException">�?fieldName �?tableName 为空时抛�?/exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛�?/exception>
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
            throw new InvalidOperationException($"获取�?{tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 检查指定SQL查询是否返回数据
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <returns>如果有数据返�?true，否则返�?false</returns>
    /// <exception cref="ArgumentException">�?sql 为空时抛�?/exception>
    /// <example>
    /// <code>
    /// // 检查用户是否存�?
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
    /// <returns>如果有数据返�?true，否则返�?false</returns>
    /// <exception cref="ArgumentException">�?sql 为空时抛�?/exception>
    /// <example>
    /// <code>
    /// // 异步检查用户是否存�?
    /// var userExists = await helper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Email = 'user@example.com'");
    /// 
    /// // 异步检查订单是否存�?
    /// var orderExists = await helper.ExistsAsync("SELECT COUNT(*) FROM Orders WHERE OrderDate >= DATEADD(day, -30, GETDATE())");
    /// 
    /// // 使用取消令牌的异步检�?
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
    /// 使用 SQL Server 特有�?BulkCopy 进行批量插入
    /// </summary>
    /// <param name="dt">数据�?/param>
    /// <returns>始终返回 true，表示成�?/returns>
    public new bool BulkInsert(DataTable dt)
    {
        if (dt?.Rows.Count > 0)
        {
            AddByBulkCopy(dt, dt.TableName);
            return true;
        }
        return false;
    }
}
