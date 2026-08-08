using System.Data;
using System.Data.Common;
using Linger.Helper;

namespace Linger.DataAccess;

/// <summary>
/// 低层数据库执行基类：负责连接/事务生命周期与 ADO.NET 命令执行。
/// </summary>
/// <remarks>
/// <para>
/// 环境事务（ambient transaction）：调用 <see cref="BeginTrans"/> 后，本实例上所有
/// <b>未显式指定连接或事务</b> 的执行方法都会自动复用该事务的连接，直到
/// <see cref="Commit"/> / <see cref="Rollback"/> / <see cref="Close"/> 结束事务。
/// </para>
/// <para>
/// 显式传入 <see cref="DbConnection"/> 或 <see cref="DbTransaction"/> 的重载始终使用调用方给定的上下文，
/// 不受环境事务影响。
/// </para>
/// <para>本类型不是线程安全的：一个实例同一时刻只应被一个逻辑操作流使用。</para>
/// <para>
/// 声明为 abstract：本类只提供执行原语，面向业务的查询语义（<c>Query</c>、<c>FindListBySql</c> 等）
/// 全在 <see cref="Database"/> 上。直接实例化拿到的是一个功能残缺的对象，请使用
/// <see cref="Database"/> 或各数据库的 Helper。
/// </para>
/// </remarks>
public abstract class BaseDatabase : IBaseDatabase
{
    /// <summary>
    /// ADO.NET 提供程序工厂，如 <c>SqlClientFactory.Instance</c>、<c>SQLiteFactory.Instance</c>。
    /// </summary>
    protected readonly DbProviderFactory Factory;

    private bool _disposed;
    private int? _commandTimeout;

    protected string ConnString { get; set; }

    /// <summary>
    /// 命令超时时间（秒）。为 null 时使用驱动默认值。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">设置为负数时抛出。</exception>
    public int? CommandTimeout
    {
        get => _commandTimeout;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            _commandTimeout = value;
        }
    }

    protected BaseDatabase(DbProviderFactory factory, string strConnection)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrWhiteSpace(strConnection);

        Factory = factory;
        ConnString = strConnection;
    }

    /// <summary>
    /// 创建并配置一个未打开的连接。
    /// </summary>
    /// <remarks>
    /// <see cref="DbProviderFactory.CreateConnection"/> 契约上可返回 null（基类默认实现即返回 null），
    /// 故在此统一收敛为异常，避免 null 扩散到各调用点。
    /// </remarks>
    /// <exception cref="InvalidOperationException">工厂不支持创建连接时抛出。</exception>
    protected DbConnection CreateConnection()
    {
        DbConnection connection = Factory.CreateConnection()
            ?? throw new InvalidOperationException(
                $"'{Factory.GetType().FullName}' did not provide a {nameof(DbConnection)}.");

        connection.ConnectionString = ConnString;
        return connection;
    }

    /// <summary>
    /// 环境事务所使用的连接对象。
    /// </summary>
    private DbConnection? Connection { get; set; }

    /// <summary>
    /// 环境事务对象。
    /// </summary>
    private DbTransaction? Trans { get; set; }

    /// <summary>
    /// 是否处于环境事务之中。
    /// </summary>
    public bool InTransaction => Trans is not null;

    #region 事务

    /// <summary>
    /// 开启环境事务。重复调用返回同一个事务对象。
    /// </summary>
    /// <remarks>
    /// 开启后，本实例上不带连接/事务参数的执行方法会自动加入该事务。
    /// </remarks>
    public DbTransaction BeginTrans()
    {
        ThrowIfDisposed();

        if (Trans is not null)
        {
            return Trans;
        }

        DbConnection conn = CreateConnection();
        try
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            Connection = conn;
            Trans = conn.BeginTransaction();
            return Trans;
        }
        catch
        {
            // 开启失败不留下半初始化状态
            Connection = null;
            Trans = null;
            conn.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 开启环境事务（异步）。
    /// </summary>
    public async Task<DbTransaction> BeginTransAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (Trans is not null)
        {
            return Trans;
        }

        DbConnection conn = CreateConnection();
        try
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            Connection = conn;
            Trans = await DbCompat.BeginTransactionAsync(conn, cancellationToken).ConfigureAwait(false);
            return Trans;
        }
        catch
        {
            Connection = null;
            Trans = null;
            conn.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 提交环境事务。不在事务中时为空操作。
    /// </summary>
    public void Commit()
    {
        DbTransaction? trans = Trans;
        if (trans is null)
        {
            return;
        }

        try
        {
            trans.Commit();
        }
        finally
        {
            ClearTransactionState();
        }
    }

    /// <summary>
    /// 提交环境事务（异步）。不在事务中时为空操作。
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        DbTransaction? trans = Trans;
        if (trans is null)
        {
            return;
        }

        try
        {
            await DbCompat.CommitAsync(trans, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ClearTransactionState();
        }
    }

    /// <summary>
    /// 回滚环境事务。不在事务中时为空操作。
    /// </summary>
    public void Rollback()
    {
        DbTransaction? trans = Trans;
        if (trans is null)
        {
            return;
        }

        try
        {
            trans.Rollback();
        }
        finally
        {
            ClearTransactionState();
        }
    }

    /// <summary>
    /// 回滚环境事务（异步）。不在事务中时为空操作。
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        DbTransaction? trans = Trans;
        if (trans is null)
        {
            return;
        }

        try
        {
            await DbCompat.RollbackAsync(trans, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ClearTransactionState();
        }
    }

    /// <summary>
    /// 关闭环境事务所用的连接。若事务尚未结束，先回滚。
    /// </summary>
    /// <remarks>
    /// 回滚失败不会抛出（本方法也走 <see cref="Dispose()"/> 路径，在那里抛异常会掩盖调用方的原始异常），
    /// 但会写入 <see cref="System.Diagnostics.Trace"/>：连接已断时数据库侧通常已自行回滚，
    /// 静默丢弃则让「事务到底有没有生效」无从判断。
    /// </remarks>
    public void Close()
    {
        // 未提交即关闭视为放弃：显式回滚，避免依赖驱动的隐式行为
        if (Trans is not null)
        {
            try
            {
                Trans.Rollback();
            }
            catch (Exception rollbackError)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "{0}.Close: rolling back the ambient transaction failed: {1}",
                    GetType().FullName, rollbackError);
            }
        }

        ClearTransactionState();
    }

    private void ClearTransactionState()
    {
        Trans?.Dispose();
        Trans = null;

        if (Connection is not null)
        {
            Connection.Close();
            Connection.Dispose();
            Connection = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // 未结束的事务在释放时回滚，而不是听任驱动处置
            Close();
        }

        _disposed = true;
    }

    /// <summary>
    /// 已释放后继续使用则抛出异常。
    /// </summary>
    /// <remarks>
    /// 释放后本实例只剩连接字符串，仍能「正常」新建连接执行命令，环境事务却已被回滚清空——
    /// 于是本该在事务里的写入会各自自动提交。宁可抛异常，也不要静默地把事务语义换掉。
    /// </remarks>
    /// <exception cref="ObjectDisposedException">实例已被释放。</exception>
    protected void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
#endif
    }

    #endregion

    #region 执行上下文解析

    /// <summary>
    /// 一次执行所使用的连接与事务，以及该连接是否由本次执行拥有。
    /// </summary>
    private readonly struct ExecutionContext(DbConnection connection, DbTransaction? transaction, bool ownsConnection)
    {
        internal DbConnection Connection { get; } = connection;
        internal DbTransaction? Transaction { get; } = transaction;

        /// <summary>为 true 时连接由本次执行创建，需由本次执行负责释放。</summary>
        internal bool OwnsConnection { get; } = ownsConnection;
    }

    /// <summary>
    /// 解析执行上下文：显式连接 &gt; 环境事务 &gt; 新建连接。
    /// </summary>
    private ExecutionContext ResolveContext(DbConnection? connection, DbTransaction? transaction)
    {
        ThrowIfDisposed();

        if (connection is not null)
        {
            return new ExecutionContext(connection, transaction, ownsConnection: false);
        }

        if (transaction is not null)
        {
            DbConnection? txConn = transaction.Connection;
            txConn.EnsureIsNotNull();
            return new ExecutionContext(txConn, transaction, ownsConnection: false);
        }

        // 环境事务：让不带参数的调用自动加入 BeginTrans 开启的事务
        if (Trans is not null && Connection is not null)
        {
            return new ExecutionContext(Connection, Trans, ownsConnection: false);
        }

        return new ExecutionContext(CreateConnection(), null, ownsConnection: true);
    }

    #endregion

    #region ExecuteNonQuery

    /// <summary>
    /// 执行命令并返回受影响行数。处于环境事务中时自动加入该事务。
    /// </summary>
    /// <param name="cmdType">命令类型（文本或存储过程）</param>
    /// <param name="cmdText">SQL 文本或存储过程名</param>
    /// <param name="parameters">命令参数</param>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteNonQueryCore(null, null, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteNonQuery(CommandType, string, DbParameter[])"/>
    public Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteNonQueryCoreAsync(null, null, cmdType, cmdText, parameters, cancellationToken);
    }

    /// <summary>
    /// 在指定连接上执行命令并返回受影响行数。
    /// </summary>
    public int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ExecuteNonQueryCore(connection, null, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteNonQuery(DbConnection, CommandType, string, DbParameter[])"/>
    public Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ExecuteNonQueryCoreAsync(connection, null, cmdType, cmdText, parameters, cancellationToken);
    }

    /// <summary>
    /// 在事务上下文中执行命令并返回受影响行数。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    public int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteNonQueryCore(null, transaction, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteNonQuery(DbTransaction, CommandType, string, DbParameter[])"/>
    public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteNonQueryCoreAsync(null, transaction, cmdType, cmdText, parameters, cancellationToken);
    }

    private int ExecuteNonQueryCore(DbConnection? connection, DbTransaction? transaction, CommandType cmdType,
        string cmdText, DbParameter[]? parameters)
    {
        ExecutionContext context = ResolveContext(connection, transaction);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                PrepareCommand(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters);
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            DisposeIfOwned(context);
        }
    }

    private async Task<int> ExecuteNonQueryCoreAsync(DbConnection? connection, DbTransaction? transaction,
        CommandType cmdType, string cmdText, DbParameter[]? parameters, CancellationToken cancellationToken)
    {
        ExecutionContext context = ResolveContext(connection, transaction);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                await PrepareCommandAsync(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters,
                    cancellationToken).ConfigureAwait(false);
                return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            await DisposeIfOwnedAsync(context).ConfigureAwait(false);
        }
    }

    #endregion

    #region ExecuteScalar

    /// <summary>
    /// 执行命令并返回首行首列。处于环境事务中时自动加入该事务。
    /// </summary>
    public object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteScalarCore(null, null, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteScalar(CommandType, string, DbParameter[])"/>
    public Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteScalarCoreAsync(null, null, cmdType, cmdText, parameters, cancellationToken);
    }

    /// <summary>
    /// 在指定连接上执行命令并返回首行首列。
    /// </summary>
    public object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ExecuteScalarCore(connection, null, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteScalar(DbConnection, CommandType, string, DbParameter[])"/>
    public Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ExecuteScalarCoreAsync(connection, null, cmdType, cmdText, parameters, cancellationToken);
    }

    /// <summary>
    /// 在事务上下文中执行命令并返回首行首列。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    public object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteScalarCore(null, transaction, cmdType, cmdText, parameters);
    }

    /// <inheritdoc cref="ExecuteScalar(DbTransaction, CommandType, string, DbParameter[])"/>
    public Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteScalarCoreAsync(null, transaction, cmdType, cmdText, parameters, cancellationToken);
    }

    private object? ExecuteScalarCore(DbConnection? connection, DbTransaction? transaction, CommandType cmdType,
        string cmdText, DbParameter[]? parameters)
    {
        ExecutionContext context = ResolveContext(connection, transaction);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                PrepareCommand(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters);
                return cmd.ExecuteScalar();
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            DisposeIfOwned(context);
        }
    }

    private async Task<object?> ExecuteScalarCoreAsync(DbConnection? connection, DbTransaction? transaction,
        CommandType cmdType, string cmdText, DbParameter[]? parameters, CancellationToken cancellationToken)
    {
        ExecutionContext context = ResolveContext(connection, transaction);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                await PrepareCommandAsync(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters,
                    cancellationToken).ConfigureAwait(false);
                return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            await DisposeIfOwnedAsync(context).ConfigureAwait(false);
        }
    }

    #endregion

    #region 内部 Reader 消费

    private protected TResult ExecuteWithReader<TResult>(CommandType cmdType, string cmdText,
        Func<DbDataReader, TResult> read, DbParameter[]? parameters)
    {
        ArgumentNullException.ThrowIfNull(read);
        ExecutionContext context = ResolveContext(null, null);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                PrepareCommand(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters);
                using DbDataReader reader = cmd.ExecuteReader();
                return read(reader);
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            DisposeIfOwned(context);
        }
    }

    private protected async Task<TResult> ExecuteWithReaderAsync<TResult>(CommandType cmdType, string cmdText,
        Func<DbDataReader, CancellationToken, Task<TResult>> read, DbParameter[]? parameters,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(read);
        ExecutionContext context = ResolveContext(null, null);
        try
        {
            using DbCommand cmd = context.Connection.CreateCommand();
            try
            {
                await PrepareCommandAsync(cmd, context.Connection, context.Transaction, cmdType, cmdText, parameters,
                    cancellationToken).ConfigureAwait(false);
                using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                return await read(reader, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ReleaseParameters(cmd);
            }
        }
        finally
        {
            await DisposeIfOwnedAsync(context).ConfigureAwait(false);
        }
    }

    #endregion

    #region GetDataSet

    /// <summary>
    /// 执行查询并把全部结果集填充到 <see cref="DataSet"/>。
    /// </summary>
    /// <remarks>
    /// 只有同步版本：BCL 的 DataSet/DataTable 填充 API 全为同步，异步化只能手写逐行循环，
    /// 不值得为此增加复杂度。异步流式读取请直接使用数据库 Provider 的 ADO.NET API。
    /// </remarks>
    public DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteWithReader(cmdType, cmdText, DataSetReader.Read, parameters);
    }

    #endregion

    #region 命令准备

    /// <summary>
    /// 准备命令：绑定连接、事务、文本、超时与参数，并确保连接已打开。
    /// </summary>
    protected void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction? transaction,
        CommandType commandType, string commandText, DbParameter[]? parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        ApplyCommand(cmd, conn, transaction, commandType, commandText, parameters);
    }

    /// <summary>
    /// 准备命令（异步）。
    /// </summary>
    protected async Task PrepareCommandAsync(DbCommand cmd, DbConnection conn, DbTransaction? transaction,
        CommandType commandType, string commandText, DbParameter[]? parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        ApplyCommand(cmd, conn, transaction, commandType, commandText, parameters);
    }

    /// <summary>
    /// 把参数从命令上摘下来，让调用方的 <see cref="DbParameter"/> 实例可以再次使用。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 部分驱动（如 <c>Microsoft.Data.SqlClient</c>）会记录参数的归属集合，
    /// 同一个 <see cref="DbParameter"/> 被加入第二个集合时抛
    /// <see cref="ArgumentException"/>（"另一个 SqlParameterCollection 中已包含 SqlParameter"）。
    /// <b>该归属关系不会因 <c>cmd.Dispose()</c> 而解除</b>，只有 <c>Parameters.Clear()</c> 能解除，
    /// 所以必须放在 <c>finally</c> 里：只在成功路径清理的话，一次失败就会让调用方的参数数组永久不可复用，
    /// 重试逻辑随之失效。
    /// </para>
    /// <para>
    /// 清理不会丢失输出参数的值：<c>Value</c> 存在 <see cref="DbParameter"/> 对象自身上，
    /// 命令执行完毕后再移出集合不影响已回填的值。
    /// </para>
    /// </remarks>
    private static void ReleaseParameters(DbCommand cmd)
    {
        // Dispose 之后访问 Parameters 会抛 ObjectDisposedException（SQLite 即如此），
        // 而本方法总在命令仍存活时调用，因此无需额外防护。
        if (cmd.Parameters.Count > 0)
        {
            cmd.Parameters.Clear();
        }
    }

    private void ApplyCommand(DbCommand cmd, DbConnection conn, DbTransaction? transaction, CommandType commandType,
        string commandText, DbParameter[]? parameters)
    {
        cmd.Connection = conn;
        cmd.CommandText = commandText;
        cmd.CommandType = commandType;

        if (CommandTimeout.HasValue)
        {
            cmd.CommandTimeout = CommandTimeout.Value;
        }

        if (transaction is not null)
        {
            cmd.Transaction = transaction;
        }

        ConfigureCommand(cmd);

        if (parameters is { Length: > 0 })
        {
            cmd.Parameters.AddRange(parameters);
        }
    }

    /// <summary>
    /// 允许具体数据库提供程序在执行前配置其专用命令属性。
    /// </summary>
    /// <param name="command">即将执行的命令。</param>
    protected virtual void ConfigureCommand(DbCommand command)
    {
    }

    #endregion

    #region 释放

    private static void DisposeIfOwned(ExecutionContext context)
    {
        if (context.OwnsConnection)
        {
            context.Connection.Dispose();
        }
    }

    private static Task DisposeIfOwnedAsync(ExecutionContext context)
    {
        return context.OwnsConnection
            ? DbCompat.DisposeAsync(context.Connection)
            : Task.CompletedTask;
    }

    #endregion
}
