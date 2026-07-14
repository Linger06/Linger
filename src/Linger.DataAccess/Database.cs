using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
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
        cancellationToken.ThrowIfCancellationRequested();
        return GetDataSetAsync(CommandType.Text, sql, parameters ?? Array.Empty<DbParameter>(), cancellationToken);
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
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.Text, sql);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(string sql, DbParameter[] parameters, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.Text, sql, parameters);
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
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.Text, sql.ToString());
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
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder sql, DbParameter[] parameters, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.Text, sql.ToString(), parameters);
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
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.StoredProcedure, procName);
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
    /// <param name="transaction">事务对象</param>
    /// <returns></returns>
    public int ExecuteByProc(string procName, DbParameter[] parameters, DbTransaction transaction)
    {
        return ExecuteNonQuery(transaction, CommandType.StoredProcedure, procName, parameters);
    }

    #endregion

    #region 查询数据列表、返回List

    /// <summary>
    ///     查询数据列表、按同名属性映射并返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public List<T> FindListBySql<T>(string sql)
    {
        using IDataReader reader = ExecuteReader(CommandType.Text, sql);
        return ReadList(reader, CreateReflectionMapper<T>(reader));
    }

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="map">数据记录映射函数</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string sql, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        using IDataReader reader = ExecuteReader(CommandType.Text, sql);
        return ReadList(reader, map);
    }

    /// <summary>
    ///     查询数据列表、按同名属性映射并返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public List<T> FindListBySql<T>(string sql, DbParameter[] parameters)
    {
        using IDataReader reader = ExecuteReader(CommandType.Text, sql, parameters);
        return ReadList(reader, CreateReflectionMapper<T>(reader));
    }

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="map">数据记录映射函数</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string sql, DbParameter[] parameters, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        using IDataReader reader = ExecuteReader(CommandType.Text, sql, parameters);
        return ReadList(reader, map);
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
        using IDataReader reader = ExecuteReader(CommandType.Text, sql);
        return ReadDataTable(reader);
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
    public async Task<DataTable> FindTableBySqlAsync(string sql)
    {
        return await FindTableBySqlAsync(sql, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    ///     查询数据列表、异步返回DataTable
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public Task<DataTable> FindTableBySqlAsync(string sql, CancellationToken cancellationToken)
    {
        return QueryTableAsync(sql, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable（异步版本）
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public async Task<DataTable> FindTableBySqlAsync(string sql, DbParameter[] parameters)
    {
        return await FindTableBySqlAsync(sql, parameters, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    ///     查询数据列表、异步返回DataTable
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public Task<DataTable> FindTableBySqlAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken)
    {
        return QueryTableAsync(sql, parameters, cancellationToken);
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataTable FindTableBySql(string sql, DbParameter[] parameters)
    {
        using IDataReader reader = ExecuteReader(CommandType.Text, sql, parameters);
        return ReadDataTable(reader);
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <returns></returns>
    public DataTable FindTableByProc(string procName)
    {
        using IDataReader reader = ExecuteReader(CommandType.StoredProcedure, procName);
        return ReadDataTable(reader);
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="procName">存储过程</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataTable FindTableByProc(string procName, DbParameter[] parameters)
    {
        using IDataReader reader = ExecuteReader(CommandType.StoredProcedure, procName, parameters);
        return ReadDataTable(reader);
    }

    private static DataTable ReadDataTable(IDataReader reader)
    {
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    private static List<T> ReadList<T>(IDataReader reader, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var result = new List<T>();
        while (reader.Read())
        {
            result.Add(map(reader));
        }

        return result;
    }

    private static T? ReadFirstOrDefault<T>(IDataReader reader, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return reader.Read() ? map(reader) : default;
    }

    private static Func<IDataRecord, T> CreateReflectionMapper<T>(IDataRecord schema)
    {
        var ordinals = new Dictionary<string, int>(schema.FieldCount, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < schema.FieldCount; i++)
        {
            ordinals[schema.GetName(i)] = i;
        }

        var bindings = ReflectionMapperCache<T>.WritableProperties
            .Where(property => ordinals.ContainsKey(property.Name))
            .Select(property => new KeyValuePair<PropertyInfo, int>(property, ordinals[property.Name]))
            .ToArray();

        return record =>
        {
            object instance = Activator.CreateInstance(typeof(T))
                ?? throw new InvalidOperationException($"Unable to create an instance of '{typeof(T).FullName}'.");

            foreach (KeyValuePair<PropertyInfo, int> binding in bindings)
            {
                object? value = record.GetValue(binding.Value);
                if (value is null or DBNull)
                    continue;

                object? convertedValue = Linger.Helper.TypeConverter.ConvertTo(value, binding.Key.PropertyType);
                binding.Key.SetValue(instance, convertedValue, null);
            }

            return (T)instance;
        };
    }

    private static class ReflectionMapperCache<T>
    {
        internal static readonly PropertyInfo[] WritableProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite &&
                               property.SetMethod?.IsPublic == true &&
                               property.GetIndexParameters().Length == 0)
            .ToArray();
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
        return GetDataSetAsync(CommandType.Text, sql, parameters, CancellationToken.None);
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
    ///     查询对象、按同名属性映射并返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <returns></returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public T? FindEntityBySql<T>(string sql)
    {
        using IDataReader reader = ExecuteReader(CommandType.Text, sql);
        return ReadFirstOrDefault(reader, CreateReflectionMapper<T>(reader));
    }

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="map">数据记录映射函数</param>
    /// <returns></returns>
    public T? FindEntityBySql<T>(string sql, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        using IDataReader reader = ExecuteReader(CommandType.Text, sql);
        return ReadFirstOrDefault(reader, map);
    }

    /// <summary>
    ///     查询对象、按同名属性映射并返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public T? FindEntityBySql<T>(string sql, DbParameter[] parameters)
    {
        using IDataReader reader = ExecuteReader(CommandType.Text, sql, parameters);
        return ReadFirstOrDefault(reader, CreateReflectionMapper<T>(reader));
    }

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="map">数据记录映射函数</param>
    /// <returns></returns>
    public T? FindEntityBySql<T>(string sql, DbParameter[] parameters, Func<IDataRecord, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        using IDataReader reader = ExecuteReader(CommandType.Text, sql, parameters);
        return ReadFirstOrDefault(reader, map);
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> FindCountBySqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        return (await ExecuteScalarAsync(CommandType.Text, sql, cancellationToken).ConfigureAwait(false)).ToIntOrDefault();
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
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task<int> FindCountBySqlAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
    {
        return (await ExecuteScalarAsync(CommandType.Text, sql, parameters, cancellationToken).ConfigureAwait(false)).ToIntOrDefault();
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
        return await ExecuteScalarAsync(CommandType.Text, sql, parameters, CancellationToken.None).ConfigureAwait(false);
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;

        do
        {
            var currentBatch = parameters.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var parameterNames = currentBatch.Select((_, index) => GetParameterName(index)).ToArray();
            var formattedSql = string.Format(CultureInfo.InvariantCulture, sql, string.Join(",", parameterNames));
            var dbParams = currentBatch.Select((value, index) => CreateParameter(GetParameterName(index), (object?)value ?? DBNull.Value)).ToArray();

            var resultDataSet = FindDataSetBySql(formattedSql, dbParams);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            AppendRows(dataTable, currentPageData);
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

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
            var formattedSql = string.Format(CultureInfo.InvariantCulture, sql, string.Join(",", parameterNames));
            var dbParams = currentBatch.Select((value, index) => CreateParameter(GetParameterName(index), (object?)value ?? DBNull.Value)).ToArray();

            var resultDataSet = await QueryAsync(formattedSql, dbParams, cancellationToken).ConfigureAwait(false);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            AppendRows(dataTable, currentPageData);
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var dataTable = new DataTable();
        var pageNumber = 1;
        int count;
        do
        {
            var currentBatch = values.Paging(pageNumber, batchSize);
            count = currentBatch.Count();
            if (count == 0) break;

            var joined = string.Join(",", currentBatch.Select(v => FormatRawValue(v, quote)));
            var formattedSql = string.Format(CultureInfo.InvariantCulture, sql, joined);
            var resultDataSet = FindDataSetBySql(formattedSql);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            AppendRows(dataTable, currentPageData);
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

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
            var formattedSql = string.Format(CultureInfo.InvariantCulture, sql, joined);
            var resultDataSet = await QueryAsync(formattedSql, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (resultDataSet.Tables.Count == 0) break;
            var currentPageData = resultDataSet.Tables[0];
            if (pageNumber == 1) dataTable = currentPageData.Clone();
            AppendRows(dataTable, currentPageData);
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

    private static void AppendRows(DataTable target, DataTable source)
    {
        foreach (DataRow row in source.Rows)
        {
            target.ImportRow(row);
        }
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
