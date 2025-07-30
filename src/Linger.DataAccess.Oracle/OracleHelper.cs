using System.Data;
using Linger.Extensions.Collection;
using Linger.Extensions.Data;
using Oracle.ManagedDataAccess.Client;

namespace Linger.DataAccess.Oracle;

public class OracleHelper(string strConnection) : Database(new OracleProvider(), strConnection)
{
    /// <summary>
    ///     拆分为多个1000,进行查询 (使用参数化查询防止SQL注入)
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为参数占位符</param>
    /// <param name="parameters">参数列表</param>
    /// <returns>查询结果DataTable</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public DataTable Page(string sql, List<string> parameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;

        do
        {
            var currentBatch = parameters.Paging(pageNumber, 1000);
            count = currentBatch.Count();

            if (count == 0)
            {
                break;
            }

            // 创建参数化查询
            var parameterNames = currentBatch.Select((_, index) => $":param{index}").ToArray();
            var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, string.Join(",", parameterNames));

            // 创建Oracle参数
            var oracleParams = currentBatch.Select((value, index) =>
                new OracleParameter($":param{index}", (object?)value ?? DBNull.Value)).ToArray();

            // 执行参数化查询
            var resultDataSet = FindDataSetBySql(formattedSql, oracleParams);
            var currentPageData = resultDataSet.Tables[0];

            // 仅在第一次的时候进行Clone
            if (pageNumber == 1)
            {
                dataTable = currentPageData.Clone();
            }

            dataTable = dataTable.Combine(currentPageData);
            pageNumber++;
        }
        while (count == 1000);

        return dataTable;
    }

    /// <summary>
    ///     拆分为多个1000,进行异步查询 (使用参数化查询防止SQL注入)
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为参数占位符</param>
    /// <param name="parameters">参数列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果DataTable</returns>
    /// <exception cref="ArgumentNullException">当sql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sql为空字符串时抛出</exception>
    public Task<DataTable> PageAsync(string sql, List<string> parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        return Task.Run(() =>
        {
            var dataTable = new DataTable();
            var pageNumber = 1;
            int count;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentBatch = parameters.Paging(pageNumber, 1000);
                count = currentBatch.Count();

                if (count == 0)
                {
                    break;
                }

                // 创建参数化查询
                var parameterNames = currentBatch.Select((_, index) => $":param{index}").ToArray();
                var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, string.Join(",", parameterNames));

                // 创建Oracle参数
                var oracleParams = currentBatch.Select((value, index) =>
                    new OracleParameter($":param{index}", (object?)value ?? DBNull.Value)).ToArray();

                // 执行参数化查询
                var resultDataSet = FindDataSetBySql(formattedSql, oracleParams);
                var currentPageData = resultDataSet.Tables[0];

                // 仅在第一次的时候进行Clone
                if (pageNumber == 1)
                {
                    dataTable = currentPageData.Clone();
                }

                dataTable = dataTable.Combine(currentPageData);
                pageNumber++;
            }
            while (count == 1000);

            return dataTable;
        }, cancellationToken);
    }

    /// <summary>
    ///     检查数据是否存在
    /// </summary>
    /// <param name="strSql">SQL查询语句</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当strSql为null时抛出</exception>
    /// <exception cref="ArgumentException">当strSql为空字符串时抛出</exception>
    public bool Exists(string strSql)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(strSql);

        var count = FindCountBySql(strSql);
        return count > 0;
    }

    /// <summary>
    ///     检查数据是否存在 (参数化查询版本)
    /// </summary>
    /// <param name="strSql">SQL查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当strSql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当strSql为空字符串时抛出</exception>
    public bool Exists(string strSql, params OracleParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(strSql);
        ArgumentNullException.ThrowIfNull(parameters);

        var count = FindCountBySql(strSql, parameters);
        return count > 0;
    }

    /// <summary>
    ///     异步检查数据是否存在
    /// </summary>
    /// <param name="strSql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当strSql为null时抛出</exception>
    /// <exception cref="ArgumentException">当strSql为空字符串时抛出</exception>
    public Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(strSql);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = FindCountBySql(strSql);
            return count > 0;
        }, cancellationToken);
    }

    /// <summary>
    ///     异步检查数据是否存在 (参数化查询版本)
    /// </summary>
    /// <param name="strSql">SQL查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当strSql或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当strSql为空字符串时抛出</exception>
    public Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default, params OracleParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(strSql);
        ArgumentNullException.ThrowIfNull(parameters);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = FindCountBySql(strSql, parameters);
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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sqlString);
        return GetDataSet(CommandType.Text, sqlString);
    }

    /// <summary>
    ///     执行查询语句，返回DataSet (参数化查询版本)
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public DataSet Query(string sqlString, params OracleParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);

        return GetDataSet(CommandType.Text, sqlString, parameters);
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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sqlString);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetDataSet(CommandType.Text, sqlString);
        }, cancellationToken);
    }

    /// <summary>
    ///     异步执行查询语句，返回DataSet (参数化查询版本)
    /// </summary>
    /// <param name="sqlString">查询语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="parameters">SQL参数</param>
    /// <returns>DataSet</returns>
    /// <exception cref="ArgumentNullException">当sqlString或parameters为null时抛出</exception>
    /// <exception cref="ArgumentException">当sqlString为空字符串时抛出</exception>
    public Task<DataSet> QueryAsync(string sqlString, CancellationToken cancellationToken = default, params OracleParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sqlString);
        ArgumentNullException.ThrowIfNull(parameters);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetDataSet(CommandType.Text, sqlString, parameters);
        }, cancellationToken);
    }
}
