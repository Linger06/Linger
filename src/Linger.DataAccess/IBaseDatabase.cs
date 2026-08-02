using System.Data;
using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
/// 基础数据库执行接口：ADO.NET 层的命令执行与事务控制。
/// </summary>
/// <remarks>
/// <para>
/// <b>环境事务</b>：调用 <see cref="BeginTrans"/> 后，本实例上不带连接/事务参数的执行方法会自动加入该事务，
/// 直到 <see cref="Commit"/> / <see cref="Rollback"/> / <see cref="Close"/> 结束事务。
/// </para>
/// <para>
/// 所有 <c>transaction</c> 重载都要求 <c>transaction.Connection</c> 不为 null。
/// 若传入已分离或已释放连接的事务对象，将抛出 <see cref="ArgumentNullException"/>。
/// </para>
/// <para>
/// <b>重载约定</b>：同步方法用 <c>params DbParameter[]</c>，异步方法每个上下文只有一个签名
/// <c>(..., DbParameter[]? parameters = null, CancellationToken cancellationToken = default)</c>。
/// 异步不用 <c>params</c>：<c>params</c> 必须是最后一个形参，与取消令牌不能并存，
/// 硬凑就得为「有参无令牌 / 有参有令牌 / 无参有令牌」各开一个重载，
/// 三倍成员换来的只是少写一对方括号。多个参数传 <c>[p1, p2]</c> 即可。
/// </para>
/// </remarks>
public interface IBaseDatabase : IDisposable
{
    /// <summary>
    /// 命令超时时间（秒）。为 null 时使用驱动默认值。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">设置为负数时抛出。</exception>
    int? CommandTimeout { get; set; }

    /// <summary>是否处于环境事务之中。</summary>
    bool InTransaction { get; }

    /// <summary>开启环境事务；已在事务中时返回同一个事务对象。</summary>
    DbTransaction BeginTrans();

    /// <summary>开启环境事务（异步）。</summary>
    Task<DbTransaction> BeginTransAsync(CancellationToken cancellationToken = default);

    /// <summary>提交环境事务；不在事务中时为空操作。</summary>
    void Commit();

    /// <summary>提交环境事务（异步）；不在事务中时为空操作。</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>回滚环境事务；不在事务中时为空操作。</summary>
    void Rollback();

    /// <summary>回滚环境事务（异步）；不在事务中时为空操作。</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>关闭环境事务的连接；若事务未结束则先回滚。</summary>
    void Close();

    #region ExecuteNonQuery

    int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    #endregion

    #region ExecuteScalar

    object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    #endregion

    #region ExecuteReader

    /// <remarks>
    /// <para>返回的读取器持有内部连接的生命周期，调用方必须释放它。</para>
    /// <para>
    /// <b>参数不可复用</b>：读取器返回后命令仍在使用参数，无法归还。传入的 <see cref="DbParameter"/>
    /// 不要再交给其它命令。<c>ExecuteNonQuery</c> / <c>ExecuteScalar</c> / <c>GetDataSet</c> 则会归还参数。
    /// </para>
    /// </remarks>
    DbDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <inheritdoc cref="ExecuteReader(CommandType, string, DbParameter[])"/>
    Task<DbDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    DbDataReader ExecuteReader(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    #endregion

    #region GetDataSet

    /// <remarks>只有同步版本：BCL 的 DataSet 填充 API 全为同步，异步场景请用 <c>ExecuteReaderAsync</c>。</remarks>
    DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    #endregion
}
