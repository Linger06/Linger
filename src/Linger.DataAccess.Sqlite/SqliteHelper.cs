using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;

namespace Linger.DataAccess.Sqlite;

/// <summary>
/// SQLite数据库操作助手类，提供SQLite特有功能和安全的数据访问方法
/// </summary>
/// <remarks>
/// <para>
/// 查询、执行、事务、存在性检查与批量事务均由 <see cref="Database"/> 提供。
/// 由于 <c>SQLiteParameter[]</c> 可协变为 <c>DbParameter[]</c>，
/// 直接把 <see cref="SQLiteParameter"/> 传给基类方法即可，本类不再重复声明参数重载。
/// </para>
/// <para>
/// 本类的方法失败时抛出异常而非返回 false/-1：调用方需要知道失败原因，
/// 静默降级会把损坏的数据库伪装成正常状态。
/// </para>
/// </remarks>
public class SqliteHelper(string connectionString) : Database(SQLiteFactory.Instance, connectionString)
{
    #region 静态工厂方法

    /// <summary>
    /// 创建文件数据库实例
    /// </summary>
    /// <param name="filePath">数据库文件路径</param>
    /// <param name="createIfNotExists">文件不存在时是否创建</param>
    /// <returns>文件数据库SqliteHelper实例</returns>
    /// <exception cref="ArgumentNullException">当filePath为null时抛出</exception>
    /// <exception cref="ArgumentException">当filePath为空字符串时抛出</exception>
    public static SqliteHelper CreateFileDatabase(string filePath, bool createIfNotExists = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var connectionString = createIfNotExists
            ? $"Data Source={filePath}"
            : $"Data Source={filePath};FailIfMissing=True";

        return new SqliteHelper(connectionString);
    }

    #endregion

    #region SQLite特有功能

    /// <summary>
    /// 获取数据库文件大小（字节）
    /// </summary>
    /// <returns>文件大小（字节）</returns>
    public long GetDatabaseSize()
    {
        var result = ExecuteScalar(CommandType.Text, DatabaseSizeSql);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private const string DatabaseSizeSql =
        "SELECT page_count * page_size FROM pragma_page_count(), pragma_page_size()";

    /// <summary>
    /// 获取数据库表列表
    /// </summary>
    /// <returns>表名列表</returns>
    public List<string> GetTableNames() => FindListBySql(TableNamesSql, static record => record.GetString(0));

    /// <summary>
    /// 异步获取数据库表列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表名列表</returns>
    public Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default) =>
        FindListBySqlAsync(TableNamesSql, static record => record.GetString(0), null, cancellationToken);

    private const string TableNamesSql =
        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";

    /// <summary>
    /// 检查表是否存在
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <returns>表是否存在</returns>
    /// <exception cref="ArgumentNullException">当tableName为null时抛出</exception>
    /// <exception cref="ArgumentException">当tableName为空字符串时抛出</exception>
    public bool TableExists(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return HasRows(TableExistsSql, new SQLiteParameter("@tableName", tableName));
    }

    /// <summary>
    /// 异步检查表是否存在
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表是否存在</returns>
    /// <exception cref="ArgumentNullException">当tableName为null时抛出</exception>
    /// <exception cref="ArgumentException">当tableName为空字符串时抛出</exception>
    public Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return HasRowsAsync(TableExistsSql, [new SQLiteParameter("@tableName", tableName)], cancellationToken);
    }

    private const string TableExistsSql = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=@tableName";

    #endregion

    #region 备份和恢复

    /// <summary>
    /// 备份数据库到指定文件
    /// </summary>
    /// <param name="backupFilePath">备份文件路径</param>
    /// <exception cref="ArgumentNullException">当backupFilePath为null时抛出</exception>
    /// <exception cref="ArgumentException">当backupFilePath为空字符串时抛出</exception>
    public void BackupDatabase(string backupFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        using var connection = new SQLiteConnection(ConnString);
        connection.Open();

        using var backup = new SQLiteConnection($"Data Source={backupFilePath}");
        backup.Open();

        connection.BackupDatabase(backup, "main", "main", -1, null, 0);
    }
    #endregion
}
