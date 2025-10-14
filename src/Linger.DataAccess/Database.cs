using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

namespace Linger.DataAccess;

/// <summary>
///     操作数据库基类
/// </summary>
public class Database(IProvider provider, string connectionString) : BaseDatabase(provider, connectionString), IDatabase
{

    /// <summary>
    /// 通用同步查询，返回DataSet
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <returns>DataSet</returns>
    public DataSet Query(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return GetDataSet(CommandType.Text, sql, parameters ?? Array.Empty<DbParameter>());
    }

    /// <summary>
    /// 通用同步查询，返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <returns>DataTable</returns>
    public DataTable QueryTable(string sql, params DbParameter[] parameters)
    {
        var ds = Query(sql, parameters);
        return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
    }

    /// <summary>
    /// 通用异步查询，返回DataSet
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>DataSet</returns>
    public Task<DataSet> QueryAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetDataSet(CommandType.Text, sql, parameters ?? Array.Empty<DbParameter>());
        }, cancellationToken);
    }

    /// <summary>
    /// 通用异步查询，返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>DataTable</returns>
    public async Task<DataTable> QueryTableAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        var ds = await QueryAsync(sql, parameters, cancellationToken).ConfigureAwait(false);
        return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
    }

    #region SqlBulkCopy大批量数据插入

    /// <summary>
    ///     大批量数据插入
    ///     基类默认不支持，需要子类重写实现具体的批量插入逻辑
    /// </summary>
    /// <param name="dt">资料表</param>
    /// <returns>插入是否成功</returns>
    public virtual bool BulkInsert(DataTable dt)
    {
        throw new NotSupportedException("当前数据库提供程序不支持批量插入操作，请使用具体的数据库实现类（如 SqlServerHelper）");
    }

    #endregion

    #region 执行SQL语句

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql)
    {
        return ExecuteNonQuery(CommandType.Text, sql);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql, DbParameter[] parameters)
    {
        return ExecuteNonQuery(CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, sql);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql, DbParameter[] parameters, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql)
    {
        return ExecuteNonQuery(CommandType.Text, sql.ToString());
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, sql.ToString());
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql, DbParameter[] parameters)
    {
        return ExecuteNonQuery(CommandType.Text, sql.ToString(), parameters);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql, DbParameter[] parameters, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, sql.ToString(), parameters);
    }

    #endregion

    #region 执行存储过程

    /// <summary>
    ///     执行存储过程
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName)
    {
        return ExecuteNonQuery(CommandType.StoredProcedure, procName);
    }

    /// <summary>
    ///     执行存储过程
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.StoredProcedure, procName);
    }

    /// <summary>
    ///     执行存储过程
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName, DbParameter[] parameters)
    {
        return ExecuteNonQuery(CommandType.StoredProcedure, procName, parameters);
    }

    /// <summary>
    ///     执行存储过程
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName, DbParameter[] parameters, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.StoredProcedure, procName, parameters);
    }

    #endregion

    #region 查询数据列表、返回List

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string sql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql);
        return dr.ReaderToList<T>();
    }

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string sql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql, parameters);
        return dr.ReaderToList<T>();
    }

    #endregion

    #region 查询数据列表、返回DataTable

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public DataTable FindTableBySql(string sql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public async Task<DataTable> FindTableBySqlAsync(string sql)
    {
        IDataReader dr = await ExecuteReaderAsync(CommandType.Text, sql).ConfigureAwait(false);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public async Task<DataTable> FindTableBySqlAsync(string sql, DbParameter[] parameters)
    {
        IDataReader dr = await ExecuteReaderAsync(CommandType.Text, sql, parameters).ConfigureAwait(false);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataTable FindTableBySql(string sql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql, parameters);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <returns></returns>
    public DataTable FindTableByProc(string procName)
    {
        IDataReader dr = ExecuteReader(CommandType.StoredProcedure, procName);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataTable FindTableByProc(string procName, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.StoredProcedure, procName, parameters);
        return dr.ReaderToDataTable();
    }

    #endregion

    #region 查询数据列表、返回DataSet

    /// <summary>
    ///     查询数据列表、返回DataSet
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public DataSet FindDataSetBySql(string sql)
    {
        return GetDataSet(CommandType.Text, sql);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataSet FindDataSetBySql(string sql, DbParameter[] parameters)
    {
        return GetDataSet(CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public Task<DataSet> FindDataSetBySqlAsync(string sql)
    {
        return GetDataSetAsync(CommandType.Text, sql);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public Task<DataSet> FindDataSetBySqlAsync(string sql, DbParameter[] parameters)
    {
        return GetDataSetAsync(CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <returns></returns>
    public DataSet FindDataSetByProc(string procName)
    {
        return GetDataSet(CommandType.StoredProcedure, procName);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataSet FindDataSetByProc(string procName, DbParameter[] parameters)
    {
        return GetDataSet(CommandType.StoredProcedure, procName, parameters);
    }

    #endregion

    #region 查询对象、返回实体

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public T FindEntityBySql<T>(string sql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql);
        return dr.ReaderToModel<T>();
    }

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public T FindEntityBySql<T>(string sql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql, parameters);
        return dr.ReaderToModel<T>();
    }

    #endregion

    #region 查询对象、返回哈希表

    /// <summary>
    ///     查询对象、返回哈希表
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public Hashtable FindHashtableBySql(string sql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql);
        return dr.ReaderToHashtable();
    }

    /// <summary>
    ///     查询对象、返回哈希表
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public Hashtable FindHashtableBySql(string sql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, sql, parameters);
        return dr.ReaderToHashtable();
    }

    #endregion

    #region 查询数据、返回条数

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public int FindCountBySql(string sql)
    {
        return ExecuteScalar(CommandType.Text, sql).ToIntOrDefault();
    }

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public async Task<int> FindCountBySqlAsync(string sql)
    {
        return (await ExecuteScalarAsync(CommandType.Text, sql).ConfigureAwait(false)).ToIntOrDefault();
    }

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int FindCountBySql(string sql, DbParameter[] parameters)
    {
        return ExecuteScalar(CommandType.Text, sql, parameters).ToIntOrDefault();
    }

    /// <summary>
    ///     查询数据、返回条数（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public async Task<int> FindCountBySqlAsync(string sql, DbParameter[] parameters)
    {
        return (await ExecuteScalarAsync(CommandType.Text, sql, parameters).ConfigureAwait(false)).ToIntOrDefault();
    }

    #endregion

    #region 查询数据、返回最大数

    /// <summary>
    ///     查询数据、返回最大数
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public object? FindMaxBySql(string sql)
    {
        return ExecuteScalar(CommandType.Text, sql);
    }

    /// <summary>
    ///     查询数据、返回最大数（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public async Task<object?> FindMaxBySqlAsync(string sql)
    {
        return await ExecuteScalarAsync(CommandType.Text, sql).ConfigureAwait(false);
    }

    /// <summary>
    ///     查询数据、返回最大数
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public object? FindMaxBySql(string sql, DbParameter[] parameters)
    {
        return ExecuteScalar(CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     查询数据、返回最大数（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public async Task<object?> FindMaxBySqlAsync(string sql, DbParameter[] parameters)
    {
        return await ExecuteScalarAsync(CommandType.Text, sql, parameters).ConfigureAwait(false);
    }

    #endregion

    #region 分批查询方法

    /// <summary>
    ///     拆分为多个批次进行查询 (使用参数化查询防止SQL注入)。默认 batchSize = 1000。
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为参数占位符</param>
    /// <param name="parameters">参数列表</param>
    /// <param name="batchSize">每批次数量(>0)，默认 1000</param>
    /// <returns>查询结果DataTable</returns>
    public virtual DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);
#if NET6_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
#else
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }
#endif

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;

        do
        {
            var currentBatch = parameters.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var parameterNames = currentBatch.Select((_, index) => GetParameterName(index)).ToArray();
            var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, string.Join(",", parameterNames));
            var dbParams = currentBatch.Select((value, index) => CreateParameter(GetParameterName(index), (object?)value ?? DBNull.Value)).ToArray();

            var resultDataSet = FindDataSetBySql(formattedSql, dbParams);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            dataTable = dataTable.Combine(currentPageData);
            pageNumber++;
        } while (count == batchSize);

        return dataTable;
    }

    /// <summary>
    ///     拆分为多个批次进行异步查询 (使用参数化查询防止SQL注入)。默认 batchSize = 1000。
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为参数占位符</param>
    /// <param name="parameters">参数列表</param>
    /// <param name="batchSize">每批次数量(>0)，默认 1000</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果DataTable</returns>
    public virtual async Task<DataTable> QueryInBatchesAsync(string sql, List<string> parameters, int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);
#if NET6_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
#else
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }
#endif

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentBatch = parameters.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var parameterNames = currentBatch.Select((_, index) => GetParameterName(index)).ToArray();
            var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, string.Join(",", parameterNames));
            var dbParams = currentBatch.Select((value, index) => CreateParameter(GetParameterName(index), (object?)value ?? DBNull.Value)).ToArray();

            var resultDataSet = await FindDataSetBySqlAsync(formattedSql, dbParams).ConfigureAwait(false);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            dataTable = dataTable.Combine(currentPageData);
            pageNumber++;
        } while (count == batchSize);

        return dataTable;
    }

    /// <summary>
    ///     拆分为多个批次进行查询 (字符串拼接方式，需自行确保输入安全)。
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为值列表占位符</param>
    /// <param name="values">值列表（例如用于 IN 查询的值）</param>
    /// <param name="batchSize">每批次数量(>0)</param>
    /// <param name="quote">是否对值加单引号（默认 true）</param>
    /// <returns>查询结果DataTable</returns>
    /// <remarks>仅适用于受信任数据来源。若 values 来自用户输入，请使用参数化方法。</remarks>
    public virtual DataTable QueryInBatchesRaw(string sql, List<string> values, int batchSize = 1000, bool quote = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(values);
#if NET6_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
#else
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }
#endif

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;
        do
        {
            var currentBatch = values.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var joined = string.Join(",", currentBatch.Select(v => FormatRawValue(v, quote)));
            var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, joined);
            var resultDataSet = FindDataSetBySql(formattedSql);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            dataTable = dataTable.Combine(currentPageData);
            pageNumber++;
        } while (count == batchSize);

        return dataTable;
    }

    /// <summary>
    ///     拆分为多个批次进行异步查询 (字符串拼接方式，需自行确保输入安全)。
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 {0} 作为值列表占位符</param>
    /// <param name="values">值列表</param>
    /// <param name="batchSize">每批次数量(>0)</param>
    /// <param name="quote">是否对值加单引号（默认 true）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果DataTable</returns>
    public virtual async Task<DataTable> QueryInBatchesRawAsync(string sql, List<string> values, int batchSize = 1000, bool quote = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(values);
#if NET6_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
#else
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }
#endif

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentBatch = values.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var joined = string.Join(",", currentBatch.Select(v => FormatRawValue(v, quote)));
            var formattedSql = string.Format(ExtensionMethodSetting.DefaultCulture, sql, joined);
            var resultDataSet = await FindDataSetBySqlAsync(formattedSql).ConfigureAwait(false);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            dataTable = dataTable.Combine(currentPageData);
            pageNumber++;
        } while (count == batchSize);

        return dataTable;
    }

    /// <summary>
    ///     格式化原始拼接值
    /// </summary>
    protected virtual string FormatRawValue(string? value, bool quote)
    {
        if (value == null) return "NULL";
        if (!quote) return value;
        return "'" + value.Replace("'", "''") + "'";
    }

    /// <summary>
    ///     获取参数名称（不同数据库使用不同的参数前缀）
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <returns>参数名称</returns>
    protected virtual string GetParameterName(int index)
    {
        // 默认使用 @ 符号（SQL Server, SQLite 兼容）
        return $"@param{index}";
    }

    /// <summary>
    ///     创建数据库参数
    /// </summary>
    protected virtual DbParameter CreateParameter(string parameterName, object value)
    {
        var command = Provider.CreateCommand();
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value;
        return parameter;
    }

    #endregion
}
