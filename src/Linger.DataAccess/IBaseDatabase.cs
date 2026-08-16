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

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    int ExecuteNonQuery(CommandType cmdType, string cmdText);
    int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText);

    /// <summary>
    /// 在事务上下文中执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    IDataReader ExecuteReader(CommandType cmdType, string cmdText);
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    IDataReader ExecuteReader(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    object? ExecuteScalar(CommandType cmdType, string cmdText);
    object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    object? ExecuteScalar(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    DataSet GetDataSet(CommandType cmdType, string cmdText);
    DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中异步执行命令，并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("ExecuteReaderAsync will be removed in 2.0.0. Use the Provider ADO.NET API instead. See the migration guide in the Linger repository for guidance.")]
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("ExecuteReaderAsync will be removed in 2.0.0. Use the Provider ADO.NET API instead. See the migration guide in the Linger repository for guidance.")]
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    [Obsolete("ExecuteReaderAsync will be removed in 2.0.0. Use the Provider ADO.NET API instead. See the migration guide in the Linger repository for guidance.")]
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    /// <summary>
    /// 在事务上下文中异步执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("ExecuteReaderAsync will be removed in 2.0.0. Use the Provider ADO.NET API instead. See the migration guide in the Linger repository for guidance.")]
    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <summary>
    /// 在事务上下文中异步执行查询并返回数据读取器。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    [Obsolete("ExecuteReaderAsync will be removed in 2.0.0. Use the Provider ADO.NET API instead. See the migration guide in the Linger repository for guidance.")]
    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("This overload will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("This overload will be removed in 2.0.0. Pass a DbParameter[] array instead of params arguments. See the migration guide in the Linger repository for guidance.")]
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    [Obsolete("GetDataSetAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    [Obsolete("GetDataSetAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    [Obsolete("GetDataSetAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

}
