using System.Data;
using System.Data.Common;
using Linger.Helper;

namespace Linger.DataAccess;

public class BaseDatabase : IBaseDatabase
{
    #region 构造函数

    protected readonly IProvider Provider;
    private bool _disposed;

    protected string ConnString { get; set; }

    public BaseDatabase(IProvider provider, string strConnection)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(strConnection);

        Provider = provider;
        ConnString = strConnection;
    }

    /// <summary>
    /// 数据库连接对象
    /// </summary>
    private DbConnection? Connection { get; set; }

    protected bool IsConnected { get; set; }

    /// <summary>
    /// 事务对象
    /// </summary>
    private DbTransaction? Trans { get; set; }
    /// <summary>
    /// 是否已在事务之中
    /// </summary>
    public bool InTransaction { get; private set; }
    /// <summary>
    /// 事务开始
    /// </summary>
    /// <returns></returns>
    public DbTransaction BeginTrans()
    {
        if (!InTransaction)
        {
            Connection = Provider.CreateConnection(ConnString);
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
            InTransaction = true;
            Trans = Connection.BeginTransaction();
        }
        return Trans!;
    }
    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit()
    {
        if (InTransaction)
        {
            InTransaction = false;
            Trans!.Commit();
            Close();
        }
    }
    /// <summary>
    /// 回滚事务
    /// </summary>
    public void Rollback()
    {
        if (InTransaction)
        {
            InTransaction = false;
            Trans!.Rollback();
            Close();
        }
    }
    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void Close()
    {
        if (Connection != null)
        {
            Connection.Close();
            Connection.Dispose();
        }
        Trans?.Dispose();
        Connection = null;
        Trans = null;
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
            Connection?.Dispose();
            Trans?.Dispose();
        }

        _disposed = true;
    }

    #endregion

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        using DbConnection conn = Provider.CreateConnection(ConnString);
        return ExecuteNonQueryInternal(conn, null, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteNonQueryAsync(cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        using DbConnection conn = Provider.CreateConnection(ConnString);
        return await ExecuteNonQueryInternalAsync(conn, null, cmdType, cmdText, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns></returns>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText)
    {
        using DbConnection conn = Provider.CreateConnection(ConnString);
        return ExecuteNonQueryInternal(conn, null, cmdType, cmdText, null);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText,
        CancellationToken cancellationToken = default)
    {
        using DbConnection conn = Provider.CreateConnection(ConnString);
        return await ExecuteNonQueryInternalAsync(conn, null, cmdType, cmdText, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        return ExecuteNonQueryInternal(connection, null, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteNonQueryAsync(connection, cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        return await ExecuteNonQueryInternalAsync(connection, null, cmdType, cmdText, parameters,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText)
    {
        return ExecuteNonQueryInternal(connection, null, cmdType, cmdText, null);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        return await ExecuteNonQueryInternalAsync(connection, null, cmdType, cmdText, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns></returns>
    public int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return ExecuteNonQueryInternal(conn, transaction, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns></returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteNonQueryAsync(transaction, cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return await ExecuteNonQueryInternalAsync(conn, transaction, cmdType, cmdText, parameters,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns></returns>
    public int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return ExecuteNonQueryInternal(conn, transaction, cmdType, cmdText, null);
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数。
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return await ExecuteNonQueryInternalAsync(conn, transaction, cmdType, cmdText, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText"> 存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(DbTransaction transaction, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return ExecuteReaderInternal(conn, transaction, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText"> 存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <exception cref="ArgumentNullException">当 transaction.Connection 为 null 时抛出。</exception>
    /// <returns>返回SqlDataReader对象</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteReaderAsync(transaction, cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return await ExecuteReaderInternalAsync(conn, transaction, cmdType, cmdText, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteReaderInternal(null, null, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteReaderAsync(cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        return await ExecuteReaderInternalAsync(null, null, cmdType, cmdText, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(CommandType cmdType, string cmdText)
    {
        return ExecuteReaderInternal(null, null, cmdType, cmdText, null);
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回SqlDataReader对象</returns>
    public async Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        return await ExecuteReaderInternalAsync(null, null, cmdType, cmdText, null, cancellationToken)
            .ConfigureAwait(false);
    }

    private int ExecuteNonQueryInternal(DbConnection connection, DbTransaction? transaction, CommandType cmdType,
        string cmdText, DbParameter[]? parameters)
    {
        using DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, transaction, cmdType, cmdText, parameters);
        var affectedRows = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return affectedRows;
    }

    private async Task<int> ExecuteNonQueryInternalAsync(DbConnection connection, DbTransaction? transaction,
        CommandType cmdType, string cmdText, DbParameter[]? parameters, CancellationToken cancellationToken)
    {
        using DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, transaction, cmdType, cmdText, parameters).ConfigureAwait(false);
        var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        cmd.Parameters.Clear();
        return affectedRows;
    }

    private IDataReader ExecuteReaderInternal(DbConnection? connection, DbTransaction? transaction,
        CommandType cmdType, string cmdText, DbParameter[]? parameters)
    {
        // 当调用方未传连接时，由当前方法创建并通过 CloseConnection 交给 reader 生命周期释放。
        var ownsConnection = connection is null;
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = connection ?? Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, transaction, cmdType, cmdText, parameters);
            var commandBehavior = ownsConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default;
            IDataReader reader = cmd.ExecuteReader(commandBehavior);
            cmd.Parameters.Clear();
            return reader;
        }
        catch (Exception)
        {
            if (ownsConnection)
            {
                conn.Close();
            }

            cmd.Dispose();
            throw;
        }
    }

    private async Task<IDataReader> ExecuteReaderInternalAsync(DbConnection? connection,
        DbTransaction? transaction, CommandType cmdType, string cmdText, DbParameter[]? parameters,
        CancellationToken cancellationToken)
    {
        // 异步路径与同步一致：仅在内部创建连接时使用 CloseConnection 绑定 reader 生命周期。
        var ownsConnection = connection is null;
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = connection ?? Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, transaction, cmdType, cmdText, parameters).ConfigureAwait(false);
            var commandBehavior = ownsConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default;
            IDataReader reader = await cmd.ExecuteReaderAsync(commandBehavior, cancellationToken)
                .ConfigureAwait(false);
            cmd.Parameters.Clear();
            return reader;
        }
        catch (Exception)
        {
            if (ownsConnection)
            {
                await CloseConnectionAsync(conn).ConfigureAwait(false);
            }

            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet中
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns>数据集DataSet对象</returns>
    public DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return FillDataSet(cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet中
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns>数据集DataSet对象</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return FillDataSetAsync(cmdType, cmdText, parameters, CancellationToken.None);
    }

    public Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        return FillDataSetAsync(cmdType, cmdText, parameters, cancellationToken);
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet中
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <returns>数据集DataSet对象</returns>
    public DataSet GetDataSet(CommandType cmdType, string cmdText)
    {
        return FillDataSet(cmdType, cmdText, null);
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet中
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据集DataSet对象</returns>
    public Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        return FillDataSetAsync(cmdType, cmdText, null, cancellationToken);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        using DbConnection connection = Provider.CreateConnection(ConnString);
        return ExecuteScalarInternal(connection, null, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteScalarAsync(cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        using DbConnection connection = Provider.CreateConnection(ConnString);
        return await ExecuteScalarInternalAsync(connection, null, cmdType, cmdText, parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(CommandType cmdType, string cmdText)
    {
        using DbConnection connection = Provider.CreateConnection(ConnString);
        return ExecuteScalarInternal(connection, null, cmdType, cmdText, null);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public async Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = Provider.CreateConnection(ConnString);
        return await ExecuteScalarInternalAsync(connection, null, cmdType, cmdText, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        return ExecuteScalarInternal(connection, null, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteScalarAsync(connection, cmdType, cmdText, parameters, CancellationToken.None);
    }

    // 数组 + CancellationToken (token 放在最后) — 实际实现
    public async Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        return await ExecuteScalarInternalAsync(connection, null, cmdType, cmdText, parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText)
    {
        return ExecuteScalarInternal(connection, null, cmdType, cmdText, null);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public async Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        return await ExecuteScalarInternalAsync(connection, null, cmdType, cmdText, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="connection">数据库连接对象</param>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(DbConnection connection, DbTransaction transaction, CommandType cmdType,
        string cmdText)
    {
        return ExecuteScalarInternal(connection, transaction, cmdType, cmdText, null);
    }

    private object? ExecuteScalarInternal(DbConnection connection, DbTransaction? transaction, CommandType cmdType,
        string cmdText, DbParameter[]? parameters)
    {
        using DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, transaction, cmdType, cmdText, parameters);
        var value = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return value;
    }

    private async Task<object?> ExecuteScalarInternalAsync(DbConnection connection, DbTransaction? transaction,
        CommandType cmdType, string cmdText, DbParameter[]? parameters, CancellationToken cancellationToken)
    {
        using DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, transaction, cmdType, cmdText, parameters).ConfigureAwait(false);
        var value = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        cmd.Parameters.Clear();
        return value;
    }

    private DataSet FillDataSet(CommandType cmdType, string cmdText, DbParameter[]? parameters)
    {
        using DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);

        PrepareCommand(cmd, conn, null, cmdType, cmdText, parameters);

        using DbDataAdapter dataAdapter = Provider.CreateDataAdapter(cmd);
        var dataSet = new DataSet();
        _ = dataAdapter.Fill(dataSet);
        return dataSet;
    }

    private async Task<DataSet> FillDataSetAsync(CommandType cmdType, string cmdText, DbParameter[]? parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);

        await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, parameters).ConfigureAwait(false);
        return await ReadDataSetAsync(cmd, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<DataSet> ReadDataSetAsync(DbCommand cmd, CancellationToken cancellationToken)
    {
        using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var dataSet = new DataSet();

        do
        {
            if (reader.FieldCount <= 0)
            {
                continue;
            }

            var table = CreateDataTable(reader, dataSet.Tables.Count);
            await PopulateTableRowsAsync(reader, table, cancellationToken).ConfigureAwait(false);
            dataSet.Tables.Add(table);
        } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));

        return dataSet;
    }

    private static DataTable CreateDataTable(DbDataReader reader, int tableIndex)
    {
        var tableName = tableIndex == 0 ? "Table" : $"Table{tableIndex}";
        var table = new DataTable(tableName);
        var usedColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
        {
            var baseColumnName = reader.GetName(columnIndex);
            if (string.IsNullOrWhiteSpace(baseColumnName))
            {
                baseColumnName = $"Column{columnIndex + 1}";
            }

            var columnName = baseColumnName;
            var duplicateSuffix = 2;
            while (!usedColumnNames.Add(columnName))
            {
                columnName = $"{baseColumnName}_{duplicateSuffix}";
                duplicateSuffix++;
            }

            Type columnType;
            try
            {
                columnType = reader.GetFieldType(columnIndex);
            }
            catch
            {
                columnType = typeof(object);
            }

            if (columnType == typeof(DBNull) || columnType == typeof(void))
            {
                columnType = typeof(object);
            }

            if (Nullable.GetUnderlyingType(columnType) is Type underlyingType)
            {
                columnType = underlyingType;
            }

            table.Columns.Add(columnName, columnType);
        }

        return table;
    }

    private static async Task PopulateTableRowsAsync(DbDataReader reader, DataTable table,
        CancellationToken cancellationToken)
    {
        var fieldCount = reader.FieldCount;
        var values = new object[fieldCount];

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            _ = reader.GetValues(values);
            var row = table.NewRow();

            for (var columnIndex = 0; columnIndex < fieldCount; columnIndex++)
            {
                row[columnIndex] = values[columnIndex] ?? DBNull.Value;
            }

            table.Rows.Add(row);
        }
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="conn">数据库连接对象</param>
    /// <param name="transaction">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public async Task<object?> ExecuteScalarAsync(DbConnection conn, DbTransaction transaction,
        CommandType cmdType, string cmdText, CancellationToken cancellationToken = default)
    {
        return await ExecuteScalarInternalAsync(conn, transaction, cmdType, cmdText, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="transaction">事务</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    public object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbConnection? conn = transaction.Connection;

        conn.EnsureIsNotNull();
        return ExecuteScalarInternal(conn, transaction, cmdType, cmdText, parameters);
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命令
    /// </summary>
    /// <param name="transaction">事务</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令行</param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型。</returns>
    // 便捷重载：支持 params，不传 CancellationToken
    public Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        return ExecuteScalarAsync(transaction, cmdType, cmdText, parameters, CancellationToken.None);
    }

    public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType,
        string cmdText, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        DbConnection? conn = transaction.Connection;
        conn.EnsureIsNotNull();
        return await ExecuteScalarInternalAsync(conn, transaction, cmdType, cmdText, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     为即将执行准备一个命令
    /// </summary>
    /// <param name="cmd">SqlCommand对象</param>
    /// <param name="conn">SqlConnection对象</param>
    /// <param name="transaction">DbTransaction对象</param>
    /// <param name="commandType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="commandText">存储过程名称或者T-SQL命令行, e.g. Select * from Products</param>
    /// <param name="parameters">SqlParameters to use in the command</param>
    /// <param name="times">超时时间 秒</param>
    protected void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction? transaction, CommandType commandType,
        string commandText, DbParameter[]? parameters, int? times = null)
    {
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        cmd.Connection = conn;
        cmd.CommandText = commandText;

        if (times != null)
        {
            cmd.CommandTimeout = (int)times;
        }

        if (transaction != null)
        {
            cmd.Transaction = transaction;
        }

        cmd.CommandType = commandType;
        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }
    }

    /// <summary>
    ///     为即将执行准备一个命令
    /// </summary>
    /// <param name="cmd">SqlCommand对象</param>
    /// <param name="conn">SqlConnection对象</param>
    /// <param name="transaction">DbTransaction对象</param>
    /// <param name="commandType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="commandText">存储过程名称或者T-SQL命令行, e.g. Select * from Products</param>
    /// <param name="parameters">SqlParameters to use in the command</param>
    /// <param name="times">超时时间 秒</param>
    protected async Task PrepareCommandAsync(DbCommand cmd, DbConnection conn, DbTransaction? transaction,
        CommandType commandType,
        string commandText, DbParameter[]? parameters, int? times = null)
    {
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync().ConfigureAwait(false);
        }

        cmd.Connection = conn;
        cmd.CommandText = commandText;

        if (times != null)
        {
            cmd.CommandTimeout = (int)times;
        }

        if (transaction != null)
        {
            cmd.Transaction = transaction;
        }

        cmd.CommandType = commandType;
        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }
    }

    private static Task CloseConnectionAsync(DbConnection conn)
    {
#if NET472 || NETSTANDARD2_0
        conn.Close();
        return Task.CompletedTask;
#else
        return conn.CloseAsync();
#endif
    }
}
