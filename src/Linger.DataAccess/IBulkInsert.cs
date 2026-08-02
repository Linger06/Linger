using System.Data;

namespace Linger.DataAccess;

/// <summary>
/// 批量插入能力。仅由原生支持批量复制的提供程序实现（如 SQL Server 的 <c>SqlBulkCopy</c>）。
/// </summary>
/// <remarks>
/// 这是一个可选能力接口，而不是 <see cref="IDatabase"/> 的一部分：
/// 不是所有数据库都有高效的批量复制机制，让 <see cref="IDatabase"/> 承诺它会导致基类实现只能抛异常。
/// 调用方应先探测能力：
/// <code>
/// if (db is IBulkInsert bulk)
/// {
///     bulk.BulkInsert(table, "Users");
/// }
/// </code>
/// </remarks>
public interface IBulkInsert
{
    /// <summary>
    /// 将内存表批量写入目标表。<paramref name="table"/> 的列名必须与目标表列名对应。
    /// </summary>
    /// <param name="table">源数据表。</param>
    /// <param name="tableName">目标表名。</param>
    /// <param name="batchSize">每批写入的行数。</param>
    /// <param name="timeout">超时时间（秒）。</param>
    /// <returns>实际写入的行数。</returns>
    int BulkInsert(DataTable table, string tableName, int batchSize = 1000, int timeout = 100);

    /// <summary>
    /// 将内存表批量写入目标表（异步）。
    /// </summary>
    /// <param name="table">源数据表。</param>
    /// <param name="tableName">目标表名。</param>
    /// <param name="batchSize">每批写入的行数。</param>
    /// <param name="timeout">超时时间（秒）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>实际写入的行数。</returns>
    Task<int> BulkInsertAsync(DataTable table, string tableName, int batchSize = 1000, int timeout = 100,
        CancellationToken cancellationToken = default);
}
