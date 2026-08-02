using System.Data;
using Linger.Extensions.Core;
using Microsoft.Data.SqlClient;

namespace Linger.DataAccess.SqlServer;

/// <summary>
/// SQL Server 数据库帮助类，提供 SQL Server 特有的功能
/// </summary>
/// <param name="connectionString">数据库连接字符串</param>
/// <remarks>
/// 通用查询、执行、事务与存在性检查由 <see cref="Database"/> 提供；
/// 由于 <c>SqlParameter[]</c> 可协变为 <c>DbParameter[]</c>，直接传入 <see cref="SqlParameter"/> 即可，
/// 无需本类再重复声明参数重载。
/// </remarks>
public class SqlServerHelper(string connectionString)
    : Database(SqlClientFactory.Instance, connectionString), IBulkInsert
{
    /// <summary>
    /// 海量数据插入方法
    /// (调用该方法需要注意，DataTable中的字段名称必须和数据库中的字段名称一一对应)
    /// </summary>
    /// <param name="table">内存表数据</param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认为 1000</param>
    /// <param name="timeout">超时时间（秒），默认为 100</param>
    /// <returns>写入的行数</returns>
    /// <exception cref="ArgumentNullException">当 table 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当 tableName 为空或包含非法字符时抛出</exception>
    public int BulkInsert(DataTable table, string tableName, int batchSize = 1000, int timeout = 100)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegative(timeout);

        // 目标表名会被直接拼进 BulkCopy 语句，必须与 GetMaxId 一样校验
        ValidateSqlIdentifier(tableName, nameof(tableName));
        var destinationTableName = QuoteQualifiedName(tableName, nameof(tableName));

        if (table.Rows.Count == 0)
        {
            return 0;
        }

        using var bulk = new SqlBulkCopy(ConnString);
        bulk.BatchSize = batchSize;
        bulk.BulkCopyTimeout = timeout;
        bulk.DestinationTableName = destinationTableName;

        bulk.WriteToServer(table);
        return table.Rows.Count;
    }

    /// <summary>
    /// 海量数据插入方法（异步版本）
    /// </summary>
    /// <param name="table">内存表数据</param>
    /// <param name="tableName">目标数据表的名称</param>
    /// <param name="batchSize">批处理大小，默认为 1000</param>
    /// <param name="timeout">超时时间（秒），默认为 100</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>写入的行数</returns>
    /// <exception cref="ArgumentNullException">当 table 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当 tableName 为空或包含非法字符时抛出</exception>
    public async Task<int> BulkInsertAsync(DataTable table, string tableName, int batchSize = 1000,
        int timeout = 100, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegative(timeout);

        ValidateSqlIdentifier(tableName, nameof(tableName));
        var destinationTableName = QuoteQualifiedName(tableName, nameof(tableName));

        if (table.Rows.Count == 0)
        {
            return 0;
        }

        using var bulk = new SqlBulkCopy(ConnString);
        bulk.BatchSize = batchSize;
        bulk.BulkCopyTimeout = timeout;
        bulk.DestinationTableName = destinationTableName;

        await bulk.WriteToServerAsync(table, cancellationToken).ConfigureAwait(false);
        return table.Rows.Count;
    }

    /// <summary>
    /// 获取指定字段的最大值并加1，通常用于生成下一个ID
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名称，可含 schema（如 <c>dbo.Users</c>）</param>
    /// <returns>最大值加 1；空表返回 1</returns>
    /// <exception cref="ArgumentException">当 fieldName 或 tableName 为空或包含非法字符时抛出</exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛出</exception>
    /// <remarks>
    /// 并发下不保证唯一，仅适合单写入者场景；需要强保证请使用 IDENTITY 或 SEQUENCE。
    /// </remarks>
    public int GetMaxId(string fieldName, string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        ValidateSqlIdentifier(fieldName, nameof(fieldName), allowQualifier: false);
        ValidateSqlIdentifier(tableName, nameof(tableName));

        try
        {
            // SQL 只取 MAX，「+1」统一由 C# 完成：空表时数据库稳定返回 NULL，加一次即可
            var sql = $"SELECT MAX([{fieldName}]) FROM {QuoteQualifiedName(tableName, nameof(tableName))}";
            var obj = ExecuteScalar(CommandType.Text, sql);

            return (obj.ToIntOrNull() ?? 0) + 1;
        }
        catch (Exception ex) when (ex is not (ArgumentException or OperationCanceledException))
        {
            throw new InvalidOperationException($"获取表 {tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 获取指定字段的最大值并加1（异步版本）
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="tableName">表名称，可含 schema（如 <c>dbo.Users</c>）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最大值加 1；空表返回 1</returns>
    /// <exception cref="ArgumentException">当 fieldName 或 tableName 为空或包含非法字符时抛出</exception>
    /// <exception cref="InvalidOperationException">当数据库操作失败时抛出</exception>
    /// <inheritdoc cref="GetMaxId(string, string)" path="/remarks"/>
    public async Task<int> GetMaxIdAsync(string fieldName, string tableName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        ValidateSqlIdentifier(fieldName, nameof(fieldName), allowQualifier: false);
        ValidateSqlIdentifier(tableName, nameof(tableName));

        try
        {
            var sql = $"SELECT MAX([{fieldName}]) FROM {QuoteQualifiedName(tableName, nameof(tableName))}";
            var obj = await ExecuteScalarAsync(CommandType.Text, sql, null, cancellationToken)
                .ConfigureAwait(false);

            return (obj.ToIntOrNull() ?? 0) + 1;
        }
        catch (Exception ex) when (ex is not (ArgumentException or OperationCanceledException))
        {
            throw new InvalidOperationException($"获取表 {tableName} 字段 {fieldName} 的最大值时发生错误", ex);
        }
    }

    /// <summary>
    /// 判断表是否存在
    /// </summary>
    /// <param name="tableName">表名称，可含 schema（如 <c>dbo.Users</c>）</param>
    /// <returns>存在返回 true</returns>
    /// <exception cref="ArgumentException">当 tableName 为空时抛出</exception>
    /// <remarks>
    /// 含 schema 时按 <c>TABLE_SCHEMA</c> + <c>TABLE_NAME</c> 精确匹配；
    /// 不含时仅按表名匹配，任意 schema 下同名表都算存在。
    /// </remarks>
    public bool TableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        (var sql, SqlParameter[] parameters) = BuildTableExistsQuery(tableName);
        return HasRows(sql, parameters);
    }

    /// <summary>
    /// 判断表是否存在（异步版本）
    /// </summary>
    /// <param name="tableName">表名称，可含 schema（如 <c>dbo.Users</c>）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存在返回 true</returns>
    /// <exception cref="ArgumentException">当 tableName 为空时抛出</exception>
    /// <inheritdoc cref="TableExists(string)" path="/remarks"/>
    public Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        (var sql, SqlParameter[] parameters) = BuildTableExistsQuery(tableName);
        return HasRowsAsync(sql, parameters, cancellationToken);
    }

    /// <summary>
    /// 按是否含 schema 构造存在性查询：<c>dbo.Users</c> 只匹配 dbo 下的 Users，
    /// 裸表名 <c>Users</c> 匹配任意 schema——原实现把 schema 削掉再查，
    /// 会让 <c>sales.Users</c> 的存在使 <c>dbo.Users</c> 误报为存在。
    /// </summary>
    private static (string Sql, SqlParameter[] Parameters) BuildTableExistsQuery(string tableName)
    {
        var separator = tableName.LastIndexOf('.');
        if (separator < 0)
        {
            return ("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName",
                [new SqlParameter("@tableName", tableName)]);
        }

        // net472 无 System.Range，用 Substring 保持多目标一致
        var schema = tableName.Substring(0, separator);
        var name = tableName.Substring(separator + 1);

        return ("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @tableName",
            [new SqlParameter("@schema", schema), new SqlParameter("@tableName", name)]);
    }

    /// <summary>
    /// 验证 SQL 标识符（表名、字段名等）是否安全
    /// </summary>
    /// <param name="identifier">要验证的标识符</param>
    /// <param name="paramName">参数名称</param>
    /// <param name="allowQualifier">是否允许点号限定（如 <c>dbo.Users</c>）。字段名应传 false。</param>
    /// <exception cref="ArgumentException">当标识符包含非法字符时抛出</exception>
    private static void ValidateSqlIdentifier(string identifier, string paramName, bool allowQualifier = true)
    {
        // 允许字母、数字、下划线和中文字符；点号仅在限定名（schema.table）中允许
        // 不允许：空格、特殊字符、SQL 关键字符（如引号、分号等）
        if (identifier.Any(c => !char.IsLetterOrDigit(c) && c != '_' && (c != '.' || !allowQualifier)))
        {
            throw new ArgumentException($"标识符 '{identifier}' 包含非法字符。只允许字母、数字、下划线和点号。", paramName);
        }

        // 防止 SQL 注入常见模式
        if (identifier.Contains("--", StringComparison.Ordinal) ||
            identifier.Contains("/*", StringComparison.Ordinal) ||
            identifier.Contains("*/", StringComparison.Ordinal) ||
            identifier.Contains(';'))
        {
            throw new ArgumentException($"标识符 '{identifier}' 包含非法的 SQL 注释或分隔符。", paramName);
        }
    }

    /// <summary>
    /// 把可含 schema 的名称逐段加方括号：<c>dbo.Users</c> → <c>[dbo].[Users]</c>。
    /// </summary>
    /// <remarks>
    /// 整体包一层 <c>[dbo.Users]</c> 是错的——SQL Server 会把它当成一张名叫「dbo.Users」的表。
    /// 调用前必须已通过 <see cref="ValidateSqlIdentifier"/>，本方法只负责切分与引用，
    /// 额外拦截空段（如 <c>dbo.</c>、<c>.Users</c>、<c>a..b</c>），它们逐段引用后会产生 <c>[]</c>。
    /// </remarks>
    /// <exception cref="ArgumentException">名称中存在空段时抛出</exception>
    private static string QuoteQualifiedName(string qualifiedName, string paramName)
    {
        var parts = qualifiedName.Split('.');
        if (parts.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException($"名称 '{qualifiedName}' 含有空段。", paramName);
        }

        return string.Join(".", parts.Select(p => $"[{p}]"));
    }
}
