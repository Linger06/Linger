using System.Data;
using Linger.Extensions.Core;
using Microsoft.Data.SqlClient;

namespace Linger.DataAccess.SqlServer;

/// <summary>
/// SQL Server 数据库帮助类，提供 SQL Server 特有的功能
/// </summary>
/// <param name="strConnection">数据库连接字符串</param>
public class SqlServerHelper(string strConnection) : Database(new SqlServerProvider(), strConnection)
{
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
        
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("表名不能为空", nameof(tableName));
        }

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
        
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("表名不能为空", nameof(tableName));
        }

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
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("字段名不能为空", nameof(fieldName));
        }
        
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("表名不能为空", nameof(tableName));
        }

        try
        {
            var strSql = $"SELECT MAX({fieldName}) + 1 FROM {tableName}";
            var obj = FindMaxBySql(strSql);

            return obj switch
            {
                null => 1,
                _ when obj.IsInt() => obj.ToInt(),
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
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("字段名不能为空", nameof(fieldName));
        }
        
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("表名不能为空", nameof(tableName));
        }

        try
        {
            var strSql = $"SELECT MAX({fieldName}) + 1 FROM {tableName}";
            var obj = await ExecuteScalarAsync(CommandType.Text, strSql).ConfigureAwait(false);

            return obj switch
            {
                null => 1,
                _ when obj.IsInt() => obj.ToInt(),
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
    /// <param name="strSql">SQL查询语句</param>
    /// <returns>如果有数据返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当 strSql 为空时抛出</exception>
    public bool Exists(string strSql)
    {
        if (string.IsNullOrWhiteSpace(strSql))
        {
            throw new ArgumentException("SQL语句不能为空", nameof(strSql));
        }
        
        var count = FindCountBySql(strSql);
        return count > 0;
    }

    /// <summary>
    /// 检查指定SQL查询是否返回数据（异步版本）
    /// </summary>
    /// <param name="strSql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>如果有数据返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当 strSql 为空时抛出</exception>
    public async Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(strSql))
        {
            throw new ArgumentException("SQL语句不能为空", nameof(strSql));
        }
        
        var count = await FindCountBySqlAsync(strSql).ConfigureAwait(false);
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
}
