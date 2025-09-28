using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Linger.DataAccess.Oracle;

public class OracleHelper(string connectionString) : Database(new OracleProvider(), connectionString)
{
    /// <summary>
    ///     获取Oracle参数名称（使用: 前缀）
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <returns>参数名称</returns>
    protected override string GetParameterName(int index)
    {
        return $":param{index}";
    }
    
    /// <summary>
    ///     检查数据是否存在
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
    ///     检查数据是否存在（参数化查询版本）
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public bool Exists(string sql, params OracleParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        var count = FindCountBySql(sql, parameters);
        return count > 0;
    }

    /// <summary>
    ///     异步检查数据是否存在
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
    ///     异步检查数据是否存在（参数化查询版本）
    /// </summary>
    /// <param name="sql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public Task<bool> ExistsAsync(string sql, CancellationToken cancellationToken = default, params OracleParameter[] parameters)
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

    /// <summary>
    ///     执行查询语句，返回DataSet
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
    ///     执行查询语句，返回DataSet （参数化查询版本）
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public DataSet Query(string sqlString, params OracleParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);
        return base.Query(sqlString, parameters);
    }

    /// <summary>
    ///     异步执行查询语句，返回DataSet
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
    ///     异步执行查询语句，返回DataSet （参数化查询版本）
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public Task<DataSet> QueryAsync(string sqlString, CancellationToken cancellationToken = default, params OracleParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);
        return base.QueryAsync(sqlString, parameters, cancellationToken);
    }
}
