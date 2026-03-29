using System.Data;
using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
/// 基础数据库执行接口。
/// </summary>
/// <remarks>
/// 所有 transaction 重载都要求 <paramref name="transaction"/> 已附着有效连接（transaction.Connection 不为 null）。
/// 若传入已分离或已释放连接的事务对象，将抛出 <see cref="ArgumentNullException"/>。
/// </remarks>
public interface IBaseDatabase : IDisposable
{
    bool InTransaction { get; }
    DbTransaction BeginTrans();
    void Commit();
    void Rollback();
    void Close();

    int ExecuteNonQuery(CommandType cmdType, string cmdText);
    int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText);

    /// <summary>
    /// 在事务上下文中执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    IDataReader ExecuteReader(CommandType cmdType, string cmdText);
    IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    IDataReader ExecuteReader(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(CommandType cmdType, string cmdText);
    object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    DataSet GetDataSet(CommandType cmdType, string cmdText);
    DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    /// <summary>
    /// 在事务上下文中异步执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中异步执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

}
