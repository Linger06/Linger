using System.Data;
using System.Data.Common;
using Linger.Helper;

namespace Linger.DataAccess;

public class BaseDatabase : IBaseDatabase
{
    #region 构造函�?

    protected readonly IProvider Provider;

    protected string ConnString { get; set; }

    public BaseDatabase(IProvider provider, string strConnection)
    {
        ArgumentNullException.ThrowIfNull(provider);
        Linger.ArgumentException.ThrowIfNullOrWhiteSpace(strConnection);

        Provider = provider;
        ConnString = strConnection;
    }

    /// <summary>
    /// 数据库连接对�?
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
    public bool InTransaction { get; set; }
    /// <summary>
    /// 事务开�?
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
    /// 关闭数据库连�?
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
        if (disposing)
        {
            Connection?.Dispose();
            Trans?.Dispose();
        }
    }

    #endregion

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);
        PrepareCommand(cmd, conn, null, cmdType, cmdText, parameters);
        var num = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);
        await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, parameters).ConfigureAwait(false);
        var num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public int ExecuteNonQuery(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);
        PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
        var num = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);
        await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, null).ConfigureAwait(false);
        var num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, null, cmdType, cmdText, parameters);
        var num = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, parameters).ConfigureAwait(false);
        var num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
        var num = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, null).ConfigureAwait(false);
        var num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbTransaction isOpenTrans, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        int num;
        DbCommand cmd = Provider.CreateCommand();
        if (isOpenTrans.Connection == null)
        {
            using DbConnection conn = Provider.CreateConnection(ConnString);
            PrepareCommand(cmd, conn, isOpenTrans, cmdType, cmdText, parameters);
            num = cmd.ExecuteNonQuery();
        }
        else
        {
            PrepareCommand(cmd, isOpenTrans.Connection, isOpenTrans, cmdType, cmdText, parameters);
            num = cmd.ExecuteNonQuery();
        }

        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        int num;
        DbCommand cmd = Provider.CreateCommand();
        if (isOpenTrans.Connection == null)
        {
            using DbConnection conn = Provider.CreateConnection(ConnString);
            await PrepareCommandAsync(cmd, conn, isOpenTrans, cmdType, cmdText, parameters).ConfigureAwait(false);
            num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        else
        {
            await PrepareCommandAsync(cmd, isOpenTrans.Connection, isOpenTrans, cmdType, cmdText,
                parameters).ConfigureAwait(false);
            num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public int ExecuteNonQuery(DbTransaction isOpenTrans, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection? conn = isOpenTrans.Connection;
        conn.EnsureIsNotNull();

        PrepareCommand(cmd, conn, isOpenTrans, cmdType, cmdText, null);
        var num = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     执行 SQL 语句，并返回受影响的行数�?
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection? conn = isOpenTrans.Connection;
        conn.EnsureIsNotNull();

        await PrepareCommandAsync(cmd, conn, isOpenTrans, cmdType, cmdText, null).ConfigureAwait(false);
        var num = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();

        return num;
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText"> 存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(DbTransaction isOpenTrans, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, isOpenTrans, cmdType, cmdText, parameters);
            IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="isOpenTrans">事务对象</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText"> 存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public async Task<IDataReader> ExecuteReaderAsync(DbTransaction isOpenTrans, CommandType cmdType,
        string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, isOpenTrans, cmdType, cmdText, parameters).ConfigureAwait(false);
            IDataReader rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, parameters);
            IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public async Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, parameters).ConfigureAwait(false);
            IDataReader rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回SqlDataReader对象</returns>
    public IDataReader ExecuteReader(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
            IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     使用提供的参数，执行有结果集返回的数据库操作命令、并返回SqlDataReader对象
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回SqlDataReader对象</returns>
    public async Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, null).ConfigureAwait(false);
            IDataReader rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns>数据集DataSet对象</returns>
    public DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        var ds = new DataSet();
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, parameters);
            var sda = Provider.CreateDataAdapter(cmd);
            _ = sda.Fill(ds);
            return ds;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns>数据集DataSet对象</returns>
    public async Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        var ds = new DataSet();
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, parameters).ConfigureAwait(false);
            var sda = Provider.CreateDataAdapter(cmd);
            _ = sda.Fill(ds);
            return ds;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <returns>数据集DataSet对象</returns>
    public DataSet GetDataSet(CommandType cmdType, string cmdText)
    {
        var ds = new DataSet();
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
            var sda = Provider.CreateDataAdapter(cmd);
            _ = sda.Fill(ds);
            return ds;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     查询数据填充到数据集DataSet�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">命令文本</param>
    /// <returns>数据集DataSet对象</returns>
    public async Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText)
    {
        var ds = new DataSet();
        DbCommand cmd = Provider.CreateCommand();
        DbConnection conn = Provider.CreateConnection(ConnString);
        try
        {
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, null).ConfigureAwait(false);
            var sda = Provider.CreateDataAdapter(cmd);
            _ = sda.Fill(ds);
            return ds;
        }
        catch (Exception)
        {
            conn.Close();
            cmd.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection connection = Provider.CreateConnection(ConnString);
        PrepareCommand(cmd, connection, null, cmdType, cmdText, parameters);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection connection = Provider.CreateConnection(ConnString);
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, parameters).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection connection = Provider.CreateConnection(ConnString);
        PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        using DbConnection connection = Provider.CreateConnection(ConnString);
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, null).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, null, cmdType, cmdText, parameters);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, parameters).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText)
    {
        DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, null, cmdType, cmdText, null).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType"></param>
    /// <param name="cmdText">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="parameters">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(DbConnection connection, DbTransaction cmdType, CommandType cmdText,
        string parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        PrepareCommand(cmd, connection, cmdType, cmdText, parameters, null);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="connection">数据库连接对�?/param>
    /// <param name="cmdType"></param>
    /// <param name="cmdText">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="parameters">存储过程名称或者T-SQL命令�?/param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(DbConnection connection, DbTransaction cmdType,
        CommandType cmdText,
        string parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        await PrepareCommandAsync(cmd, connection, cmdType, cmdText, parameters, null).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="isOpenTrans">事务</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public object? ExecuteScalar(DbTransaction isOpenTrans, CommandType cmdType, string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection? conn = isOpenTrans.Connection;

        conn.EnsureIsNotNull();

        PrepareCommand(cmd, conn, isOpenTrans, cmdType, cmdText, parameters);
        var val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     依靠数据库连接字符串strConnection,
    ///     使用所提供参数，执行返回首行首列命�?
    /// </summary>
    /// <param name="isOpenTrans">事务</param>
    /// <param name="cmdType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="cmdText">存储过程名称或者T-SQL命令�?/param>
    /// <param name="parameters">执行命令所需的sql语句对应参数</param>
    /// <returns>返回一个对象，使用Convert.To{Type}将该对象转换成想要的数据类型�?/returns>
    public async Task<object?> ExecuteScalarAsync(DbTransaction isOpenTrans, CommandType cmdType,
        string cmdText,
        params DbParameter[] parameters)
    {
        DbCommand cmd = Provider.CreateCommand();
        DbConnection? conn = isOpenTrans.Connection;
        conn.EnsureIsNotNull();

        await PrepareCommandAsync(cmd, conn, isOpenTrans, cmdType, cmdText, parameters).ConfigureAwait(false);
        var val = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    ///     为即将执行准备一个命�?
    /// </summary>
    /// <param name="cmd">SqlCommand对象</param>
    /// <param name="conn">SqlConnection对象</param>
    /// <param name="isOpenTrans">DbTransaction对象</param>
    /// <param name="commandType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="commandText">存储过程名称或者T-SQL命令�? e.g. Select * from Products</param>
    /// <param name="parameters">SqlParameters to use in the command</param>
    /// <param name="times">超时时间 �?/param>
    protected void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction? isOpenTrans, CommandType commandType,
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

        if (isOpenTrans != null)
        {
            cmd.Transaction = isOpenTrans;
        }

        cmd.CommandType = commandType;
        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }
    }

    /// <summary>
    ///     为即将执行准备一个命�?
    /// </summary>
    /// <param name="cmd">SqlCommand对象</param>
    /// <param name="conn">SqlConnection对象</param>
    /// <param name="isOpenTrans">DbTransaction对象</param>
    /// <param name="commandType">执行命令的类型（存储过程或T-SQL，等等）</param>
    /// <param name="commandText">存储过程名称或者T-SQL命令�? e.g. Select * from Products</param>
    /// <param name="parameters">SqlParameters to use in the command</param>
    /// <param name="times">超时时间 �?/param>
    protected async Task PrepareCommandAsync(DbCommand cmd, DbConnection conn, DbTransaction? isOpenTrans,
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

        if (isOpenTrans != null)
        {
            cmd.Transaction = isOpenTrans;
        }

        cmd.CommandType = commandType;
        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }
    }
}
