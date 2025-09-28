using System.Data;
using System.Data.SQLite;
using Linger.Extensions.Data;

namespace Linger.DataAccess.Sqlite;

/// <summary>
/// SQLite数据库操作助手类，提供SQLite特有功能和安全的数据访问方法
/// </summary>
public class SqliteHelper(string connectionString) : Database(new SqliteProvider(), connectionString)
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

    #region 存在性检查方法

    /// <summary>
    /// 检查数据是否存在
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public bool Exists(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        var count = FindCountBySql(sql);
        return count > 0;
    }

    /// <summary>
    /// 检查数据是否存在 (参数化查询版本)
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public bool Exists(string sql, params SQLiteParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        var count = FindCountBySql(sql, parameters);
        return count > 0;
    }

    /// <summary>
    /// 异步检查数据是否存在
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public Task<bool> ExistsAsync(string sql, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = FindCountBySql(sql);
            return count > 0;
        }, cancellationToken);
    }

    /// <summary>
    /// 异步检查数据是否存在 (参数化查询版本)
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public Task<bool> ExistsAsync(string sql, CancellationToken cancellationToken = default, params SQLiteParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = FindCountBySql(sql, parameters);
            return count > 0;
        }, cancellationToken);
    }

    #endregion

    #region 查询方法

    /// <summary>
    /// 执行查询语句，返回DataSet
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public DataSet Query(string sqlString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        return base.Query(sqlString);
    }

    /// <summary>
    /// 执行查询语句，返回DataSet (参数化查询版本)
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public DataSet Query(string sqlString, params SQLiteParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);
        return base.Query(sqlString, parameters);
    }

    /// <summary>
    /// 异步执行查询语句，返回DataSet
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public Task<DataSet> QueryAsync(string sqlString, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        return base.QueryAsync(sqlString, null, cancellationToken);
    }

    /// <summary>
    /// 异步执行查询语句，返回DataSet (参数化查询版本)
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public Task<DataSet> QueryAsync(string sqlString, CancellationToken cancellationToken = default, params SQLiteParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);
        return base.QueryAsync(sqlString, parameters, cancellationToken);
    }

    #endregion

    #region SQLite特有功能

    /// <summary>
    /// 获取数据库文件大小（字节）
    /// </summary>
    /// <returns>文件大小，如果获取失败返回-1</returns>
    public long GetDatabaseSize()
    {
        try
        {
            var result = ExecuteScalar(CommandType.Text, "SELECT page_count * page_size FROM pragma_page_count(), pragma_page_size()");
            return Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 异步获取数据库文件大小（字节）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件大小，如果获取失败返回-1</returns>
    public Task<long> GetDatabaseSizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetDatabaseSize();
        }, cancellationToken);
    }

    /// <summary>
    /// 执行VACUUM命令压缩数据库
    /// </summary>
    /// <returns>操作是否成功</returns>
    public bool VacuumDatabase()
    {
        try
        {
            ExecuteBySql("VACUUM");
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 异步执行VACUUM命令压缩数据库
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    public Task<bool> VacuumDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return VacuumDatabase();
        }, cancellationToken);
    }

    /// <summary>
    /// 分析数据库以优化查询性能
    /// </summary>
    /// <returns>操作是否成功</returns>
    public bool AnalyzeDatabase()
    {
        try
        {
            ExecuteBySql("ANALYZE");
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 异步分析数据库以优化查询性能
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    public Task<bool> AnalyzeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return AnalyzeDatabase();
        }, cancellationToken);
    }

    /// <summary>
    /// 检查数据库完整性
    /// </summary>
    /// <returns>完整性检查结果，"ok"表示正常</returns>
    public string CheckIntegrity()
    {
        try
        {
            var result = ExecuteScalar(CommandType.Text, "PRAGMA integrity_check");
            return result?.ToString() ?? "unknown";
        }
        catch (Exception ex)
        {
            return $"error: {ex.Message}";
        }
    }

    /// <summary>
    /// 异步检查数据库完整性
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完整性检查结果，"ok"表示正常</returns>
    public Task<string> CheckIntegrityAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return CheckIntegrity();
        }, cancellationToken);
    }

    /// <summary>
    /// 获取数据库表列表
    /// </summary>
    /// <returns>表名列表</returns>
    public List<string> GetTableNames()
    {
        try
        {
            var dataSet = Query("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name");
            var tableNames = new List<string>();

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var name = row["name"].ToString();
                    if (name is not null)
                    {
                        tableNames.Add(name);
                    }
                }
            }

            return tableNames;
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 异步获取数据库表列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表名列表</returns>
    public Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetTableNames();
        }, cancellationToken);
    }

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

        return Exists("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName",
            new SQLiteParameter("@tableName", tableName));
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

        return ExistsAsync("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName",
            cancellationToken, new SQLiteParameter("@tableName", tableName));
    }

    #endregion

    #region 备份和恢复

    /// <summary>
    /// 备份数据库到指定文件
    /// </summary>
    /// <param name="backupFilePath">备份文件路径</param>
    /// <returns>操作是否成功</returns>
    /// <exception cref="ArgumentNullException">当backupFilePath为null时抛出</exception>
    /// <exception cref="ArgumentException">当backupFilePath为空字符串时抛出</exception>
    public bool BackupDatabase(string backupFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        try
        {
            using var connection = new SQLiteConnection(ConnString);
            connection.Open();

            using var backup = new SQLiteConnection($"Data Source={backupFilePath}");
            backup.Open();

            connection.BackupDatabase(backup, "main", "main", -1, null, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 异步备份数据库到指定文件
    /// </summary>
    /// <param name="backupFilePath">备份文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    /// <exception cref="ArgumentNullException">当backupFilePath为null时抛出</exception>
    /// <exception cref="ArgumentException">当backupFilePath为空字符串时抛出</exception>
    public Task<bool> BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return BackupDatabase(backupFilePath);
        }, cancellationToken);
    }

    #endregion

    #region 事务辅助方法

    /// <summary>
    /// 在事务中执行多个SQL语句
    /// </summary>
    /// <param name="sqlStatements">SQL语句列表</param>
    /// <returns>操作是否成功</returns>
    /// <exception cref="ArgumentNullException">当sqlStatements为null时抛出</exception>
    public bool ExecuteInTransaction(IEnumerable<string> sqlStatements)
    {
        ArgumentNullException.ThrowIfNull(sqlStatements);

        var statements = sqlStatements.ToList();
        if (statements.Count == 0)
        {
            return true;
        }

        try
        {
            using var connection = new SQLiteConnection(ConnString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var sql in statements)
                {
                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        using var command = new SQLiteCommand(sql, connection, transaction);
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 异步在事务中执行多个SQL语句
    /// </summary>
    /// <param name="sqlStatements">SQL语句列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    /// <exception cref="ArgumentNullException">当sqlStatements为null时抛出</exception>
    public Task<bool> ExecuteInTransactionAsync(IEnumerable<string> sqlStatements, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sqlStatements);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ExecuteInTransaction(sqlStatements);
        }, cancellationToken);
    }

    #endregion
}
