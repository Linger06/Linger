using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Linger.Extensions.Core;
using Linger.Helper;

namespace Linger.DataAccess;

/// <summary>
///     操作数据库基类：在 <see cref="BaseDatabase"/> 之上提供面向业务的查询与执行语义。
/// </summary>
public class Database(DbProviderFactory factory, string connectionString)
    : BaseDatabase(factory, connectionString), IDatabase
{
    #region 通用查询

    /// <summary>
    /// 执行查询并返回 <see cref="DataSet"/>（含全部结果集）。
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <remarks>
    /// <para>
    /// DataSet / DataTable 查询只有同步版本：BCL 的填充 API 全为同步，
    /// 异步化只能手写逐行循环，不值得为此增加复杂度。
    /// 异步场景请用 <see cref="BaseDatabase.ExecuteReaderAsync(CommandType, string, DbParameter[], CancellationToken)"/> 自行读取。
    /// </para>
    /// <para>存储过程版本见 <see cref="FindDataSetByProc"/>。</para>
    /// </remarks>
    public DataSet Query(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return GetDataSet(CommandType.Text, sql, parameters);
    }

    /// <summary>
    /// 执行查询并返回首个结果集的 <see cref="DataTable"/>。
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <remarks>存储过程版本见 <see cref="FindTableByProc"/>。</remarks>
    public DataTable QueryTable(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        // 直接走 reader，避免构造整个 DataSet 后只取首表（其余表与 DataSet 会被返回值根住无法回收）
        return ExecuteReader(CommandType.Text, sql, parameters, ReadDataTable);
    }

    #endregion

    #region 批量事务执行

    /// <summary>
    /// 在单个事务中依次执行多条参数化 SQL 语句：全部成功则提交，任一条失败则整体回滚。
    /// </summary>
    /// <param name="statements">待执行的 SQL 与参数集合。空集合为空操作，返回空数组。</param>
    /// <returns>与 <paramref name="statements"/> 顺序一致的每条语句受影响行数。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="statements"/> 或集合中的语句为 null 时抛出。</exception>
    /// <remarks>
    /// <para>
    /// 失败时先回滚再把原异常向上抛出，异常不会被吞掉；
    /// 若回滚本身也失败，回滚异常会作为 <see cref="AggregateException"/> 与原异常一并抛出。
    /// </para>
    /// <para>
    /// 返回数组与入参一一对应，所以空白语句会抛异常而不是被跳过。
    /// 若语句来自脚本切分（如 <c>script.Split(';')</c>），请先自行过滤空白项。
    /// </para>
    /// <para>
    /// 本方法自行管理连接与事务。<b>处于环境事务中（<see cref="BaseDatabase.InTransaction"/> 为 true）时抛
    /// <see cref="InvalidOperationException"/></b>：本方法会另开一条连接，它与环境事务是两个互不可见的事务，
    /// 一旦触及同一批行就会等对方释放锁而死锁在自己身上。需要把这些语句纳入环境事务，
    /// 请改用 <see cref="ExecuteBySql(string, DbParameter[])"/> 逐条执行。
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">调用方已通过 <see cref="BaseDatabase.BeginTrans"/> 开启环境事务。</exception>
    /// <example>
    /// <code>
    /// var amount1 = factory.CreateParameter();
    /// amount1.ParameterName = "@amount";
    /// amount1.Value = 100;
    /// var id1 = factory.CreateParameter();
    /// id1.ParameterName = "@id";
    /// id1.Value = 1;
    ///
    /// var amount2 = factory.CreateParameter();
    /// amount2.ParameterName = "@amount";
    /// amount2.Value = 100;
    /// var id2 = factory.CreateParameter();
    /// id2.ParameterName = "@id";
    /// id2.Value = 2;
    ///
    /// var affected = db.ExecuteTransaction(new[]
    /// {
    ///     new SqlStatement("UPDATE Accounts SET Balance = Balance - @amount WHERE Id = @id",
    ///         amount1, id1),
    ///     new SqlStatement("UPDATE Accounts SET Balance = Balance + @amount WHERE Id = @id",
    ///         amount2, id2),
    /// });
    /// </code>
    /// </example>
    public int[] ExecuteTransaction(IEnumerable<SqlStatement> statements)
    {
        ArgumentNullException.ThrowIfNull(statements);
        ThrowIfAmbientTransaction();

        var items = Materialize(statements);
        if (items.Length == 0)
        {
            return [];
        }

        using DbConnection conn = CreateConnection();
        conn.Open();

        DbTransaction transaction = conn.BeginTransaction();
        try
        {
            var affected = new int[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                affected[i] = ExecuteNonQuery(transaction, CommandType.Text, items[i].Sql, items[i].Parameters);
            }

            transaction.Commit();
            return affected;
        }
        catch (Exception executionError)
        {
            RollbackOrThrowAggregate(transaction, executionError);
            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    /// <summary>
    /// 在单个事务中依次执行多条参数化 SQL 语句（异步）：全部成功则提交，任一条失败则整体回滚。
    /// </summary>
    /// <param name="statements">待执行的 SQL 与参数集合。空集合为空操作，返回空数组。</param>
    /// <param name="cancellationToken">取消令牌。取消时事务会被回滚。</param>
    /// <returns>与 <paramref name="statements"/> 顺序一致的每条语句受影响行数。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="statements"/> 或集合中的语句为 null 时抛出。</exception>
    /// <exception cref="InvalidOperationException">调用方已通过 <see cref="BaseDatabase.BeginTrans"/> 开启环境事务。</exception>
    /// <inheritdoc cref="ExecuteTransaction(IEnumerable{SqlStatement})" path="/remarks"/>
    public async Task<int[]> ExecuteTransactionAsync(IEnumerable<SqlStatement> statements,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(statements);
        ThrowIfAmbientTransaction();

        var items = Materialize(statements);
        if (items.Length == 0)
        {
            return [];
        }

        using DbConnection conn = CreateConnection();
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        DbTransaction transaction = await DbCompat.BeginTransactionAsync(conn, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var affected = new int[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                affected[i] = await ExecuteNonQueryAsync(transaction, CommandType.Text, items[i].Sql,
                    items[i].Parameters,
                    cancellationToken).ConfigureAwait(false);
            }

            await DbCompat.CommitAsync(transaction, cancellationToken).ConfigureAwait(false);
            return affected;
        }
        catch (Exception executionError)
        {
            await RollbackOrThrowAggregateAsync(transaction, executionError).ConfigureAwait(false);
            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    /// <summary>
    /// 拒绝在环境事务内再开一个独立事务。
    /// </summary>
    /// <remarks>
    /// 本方法族自带连接与事务，与环境事务分属两个事务，彼此看不到对方未提交的行。
    /// 若两者写同一批行，新事务会阻塞等待环境事务释放锁，而环境事务正等着本方法返回——
    /// 单线程上自己把自己锁死，表现为超时而非报错。所以在入口直接拒绝，而不是留在文档里提醒。
    /// </remarks>
    /// <exception cref="InvalidOperationException">已处于环境事务中。</exception>
    private void ThrowIfAmbientTransaction()
    {
        if (InTransaction)
        {
            throw new InvalidOperationException(
                "ExecuteTransaction cannot run inside an ambient transaction started by BeginTrans: " +
                "it would open a second, independent transaction on another connection and could deadlock " +
                "against the uncommitted rows of the ambient one. " +
                "Execute the statements with ExecuteBySql so they join the ambient transaction, " +
                "or commit/roll back before calling ExecuteTransaction.");
        }
    }

    /// <summary>
    /// 一次性物化并校验语句集合：返回数组要与入参下标对齐，null 项必须报错而不能跳过。
    /// </summary>
    private static SqlStatement[] Materialize(IEnumerable<SqlStatement> statements)
    {
        var items = statements as SqlStatement[] ?? [.. statements];

        for (var i = 0; i < items.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(items[i], nameof(statements));
        }

        return items;
    }

    /// <summary>
    /// 回滚事务。回滚成功则原样返回（由调用方 rethrow 原异常）；回滚失败则抛出聚合异常。
    /// </summary>
    private static void RollbackOrThrowAggregate(DbTransaction transaction, Exception executionError)
    {
        try
        {
            transaction.Rollback();
        }
        catch (Exception rollbackError)
        {
            throw new AggregateException(
                "Statement execution failed and the subsequent rollback also failed.",
                executionError, rollbackError);
        }
    }

    private static async Task RollbackOrThrowAggregateAsync(DbTransaction transaction, Exception executionError)
    {
        try
        {
            // 回滚不受原调用的取消令牌影响：令牌已取消时还要能把事务收干净
            await DbCompat.RollbackAsync(transaction, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception rollbackError)
        {
            throw new AggregateException(
                "Statement execution failed and the subsequent rollback also failed.",
                executionError, rollbackError);
        }
    }

    #endregion

    #region 存在性检查

    /// <summary>
    /// 判断查询是否返回任何行。
    /// </summary>
    /// <param name="sql">任意 SELECT 语句，例如 <c>SELECT 1 FROM Users WHERE Id = @id</c>。</param>
    /// <param name="parameters">SQL 参数。</param>
    /// <returns>至少返回一行时为 true。</returns>
    /// <remarks>
    /// 本方法只关心 <b>有没有行</b>，不解释列值，因此 <c>SELECT Id FROM ... WHERE Id = 0</c>
    /// 命中时同样返回 true。需要标量计数时请用 <see cref="FindCountBySql(string, DbParameter[])"/>。
    /// 只读取第一行即停止，配合 <c>SELECT 1 FROM ... WHERE ...</c> 比 <c>COUNT(*)</c> 更省。
    /// </remarks>
    public bool HasRows(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        return ExecuteReader(CommandType.Text, sql, parameters, static reader => reader.Read());
    }

    /// <summary>
    /// 判断查询是否返回任何行（异步）。
    /// </summary>
    /// <inheritdoc cref="HasRows(string, DbParameter[])" path="/remarks"/>
    public Task<bool> HasRowsAsync(string sql, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        return ExecuteReaderAsync(CommandType.Text, sql, parameters,
            static (reader, ct) => reader.ReadAsync(ct), cancellationToken);
    }

    #endregion

    #region 执行SQL语句

    /// <summary>
    ///     执行SQL语句。处于环境事务中时自动加入该事务。
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    public int ExecuteBySql(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ExecuteNonQuery(CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     执行SQL语句（异步）。处于环境事务中时自动加入该事务。
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task<int> ExecuteBySqlAsync(string sql, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ExecuteNonQueryAsync(CommandType.Text, sql, parameters, cancellationToken);
    }

    /// <summary>
    ///     在指定事务中执行SQL语句。
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="transaction">事务对象</param>
    /// <param name="parameters">sql语句对应参数</param>
    public int ExecuteBySql(string sql, DbTransaction transaction, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteNonQuery(transaction, CommandType.Text, sql, parameters);
    }

    /// <summary>
    ///     在指定事务中执行SQL语句（异步）。
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="transaction">事务对象</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task<int> ExecuteBySqlAsync(string sql, DbTransaction transaction, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteNonQueryAsync(transaction, CommandType.Text, sql, parameters, cancellationToken);
    }

    #endregion

    #region 执行存储过程

    /// <summary>
    ///     执行存储过程。处于环境事务中时自动加入该事务。
    /// </summary>
    /// <param name="procName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    public int ExecuteByProc(string procName, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procName);
        return ExecuteNonQuery(CommandType.StoredProcedure, procName, parameters);
    }

    /// <summary>
    ///     执行存储过程（异步）。处于环境事务中时自动加入该事务。
    /// </summary>
    /// <param name="procName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task<int> ExecuteByProcAsync(string procName, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procName);
        return ExecuteNonQueryAsync(CommandType.StoredProcedure, procName, parameters, cancellationToken);
    }

    /// <summary>
    ///     在指定事务中执行存储过程。
    /// </summary>
    /// <param name="procName">存储过程名</param>
    /// <param name="transaction">事务对象</param>
    /// <param name="parameters">存储过程参数</param>
    public int ExecuteByProc(string procName, DbTransaction transaction, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procName);
        ArgumentNullException.ThrowIfNull(transaction);
        return ExecuteNonQuery(transaction, CommandType.StoredProcedure, procName, parameters);
    }

    #endregion

    #region 查询数据列表、返回List

    /// <summary>
    ///     查询数据列表、按同名属性映射并返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <remarks>大结果集建议使用接受 <c>map</c> 的重载，避免按名映射的额外开销。</remarks>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public List<T> FindListBySql<T>(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ExecuteReader(CommandType.Text, sql, parameters,
            static reader => ReadList(reader, CreateMapper<T>(reader)));
    }

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="map">数据记录映射函数</param>
    /// <param name="parameters">sql语句对应参数</param>
    public List<T> FindListBySql<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(map);
        return ExecuteReader(CommandType.Text, sql, parameters, reader => ReadList(reader, map));
    }

    /// <summary>
    /// 异步查询数据列表、按同名属性映射并返回 List。
    /// </summary>
    /// <param name="sql">SQL 语句。</param>
    /// <param name="parameters">SQL 参数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public Task<List<T>> FindListBySqlAsync<T>(string sql, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ExecuteReaderAsync(CommandType.Text, sql, parameters,
            static (reader, ct) => ReadListAsync(reader, CreateMapper<T>(reader), ct), cancellationToken);
    }

    /// <summary>
    /// 异步查询数据列表、使用指定映射函数返回 List。
    /// </summary>
    /// <param name="sql">SQL 语句。</param>
    /// <param name="map">数据记录映射函数。</param>
    /// <param name="parameters">SQL 参数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public Task<List<T>> FindListBySqlAsync<T>(string sql, Func<IDataRecord, T> map,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(map);
        return ExecuteReaderAsync(CommandType.Text, sql, parameters,
            (reader, ct) => ReadListAsync(reader, map, ct), cancellationToken);
    }

    #endregion

    #region 查询数据列表、返回DataTable

    /// <summary>
    ///     执行存储过程、返回 DataTable
    /// </summary>
    /// <param name="procName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    public DataTable FindTableByProc(string procName, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procName);
        return ExecuteReader(CommandType.StoredProcedure, procName, parameters, ReadDataTable);
    }

    private static DataTable ReadDataTable(DbDataReader reader)
    {
        var table = new DataTable { Locale = CultureInfo.InvariantCulture };
        table.Load(reader);
        return table;
    }

    private static List<T> ReadList<T>(DbDataReader reader, Func<IDataRecord, T> map)
    {
        var result = new List<T>();
        while (reader.Read())
        {
            result.Add(map(reader));
        }

        return result;
    }

    private static async Task<List<T>> ReadListAsync<T>(DbDataReader reader, Func<IDataRecord, T> map,
        CancellationToken cancellationToken)
    {
        var result = new List<T>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(map(reader));
        }

        return result;
    }

    private static T? ReadFirstOrDefault<T>(DbDataReader reader, Func<IDataRecord, T> map)
    {
        return reader.Read() ? map(reader) : default;
    }

    #endregion

    #region 查询数据列表、返回DataSet

    /// <summary>
    ///     执行存储过程、返回DataSet
    /// </summary>
    /// <param name="procName">存储过程名</param>
    /// <param name="parameters">存储过程参数</param>
    public DataSet FindDataSetByProc(string procName, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procName);
        return GetDataSet(CommandType.StoredProcedure, procName, parameters);
    }

    #endregion

    #region 查询对象、返回实体

    /// <summary>
    ///     查询对象、按同名属性映射并返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    public T? FindEntityBySql<T>(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ExecuteReader(CommandType.Text, sql, parameters,
            static reader => ReadFirstOrDefault(reader, CreateMapper<T>(reader)));
    }

    /// <summary>
    /// 依据读取器当前架构创建按同名属性映射的委托。
    /// </summary>
    /// <remarks>
    /// 可写属性列表按类型缓存，列序号按当前架构解析一次，因此每行只做
    /// <see cref="PropertyInfo.SetValue(object, object)"/>，不再重复查找元数据。
    /// 仅支持具有公共无参构造的实体类型；标量类型或需要更高吞吐时请使用接受 <c>map</c> 的重载。
    /// </remarks>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Maps columns to properties by name using reflection metadata.")]
#endif
    private static Func<IDataRecord, T> CreateMapper<T>(IDataRecord schema)
    {
        var ordinals = new Dictionary<string, int>(schema.FieldCount, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < schema.FieldCount; i++)
        {
            // 重名列以首次出现为准，与 DataSet 的按名取值语义一致
            var name = schema.GetName(i);
            if (!ordinals.ContainsKey(name))
            {
                ordinals[name] = i;
            }
        }

        var plan = new List<(PropertyInfo Property, int Ordinal)>();
        foreach (PropertyInfo property in GetWritableProperties(typeof(T)))
        {
            if (ordinals.TryGetValue(property.Name, out var ordinal))
            {
                plan.Add((property, ordinal));
            }
        }

        (PropertyInfo Property, int Ordinal)[] bindings = [.. plan];

        return record =>
        {
            var instance = Activator.CreateInstance<T>();

            // 值类型经装箱后赋值，最后拆箱返回，否则 SetValue 写入的是副本
            object boxed = instance!;

            foreach ((PropertyInfo property, var ordinal) in bindings)
            {
                var value = record.GetValue(ordinal);
                if (value is null or DBNull)
                {
                    continue;
                }

                if (!TypeConverter.TryConvert(value, property.PropertyType, out var convertedValue))
                {
                    throw new InvalidCastException(
                        $"Cannot convert value of type '{value.GetType().Name}' to property '{property.Name}' of type '{property.PropertyType.Name}'.");
                }

                property.SetValue(boxed, convertedValue);
            }

            return (T)boxed;
        };
    }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Enumerates public instance properties.")]
#endif
    private static PropertyInfo[] GetWritableProperties(Type type)
    {
        return s_writableProperties.GetOrAdd(type, static t => [.. t
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0)]);
    }

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_writableProperties = new();

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="map">数据记录映射函数</param>
    /// <param name="parameters">sql语句对应参数</param>
    public T? FindEntityBySql<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(map);
        return ExecuteReader(CommandType.Text, sql, parameters, reader => ReadFirstOrDefault(reader, map));
    }

    #endregion

    #region 查询数据、返回条数

    /// <summary>
    ///     执行计数查询并返回条数。
    /// </summary>
    /// <param name="sql">返回单个计数值的 SQL</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns>计数值；查询无行或返回 NULL 时为 0。</returns>
    public int FindCountBySql(string sql, params DbParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return ToCount(ExecuteScalar(CommandType.Text, sql, parameters));
    }

    /// <summary>
    ///     执行计数查询并返回条数（异步）。
    /// </summary>
    /// <param name="sql">返回单个计数值的 SQL</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>计数值；查询无行或返回 NULL 时为 0。</returns>
    public async Task<int> FindCountBySqlAsync(string sql, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        var scalar = await ExecuteScalarAsync(CommandType.Text, sql, parameters, cancellationToken)
            .ConfigureAwait(false);
        return ToCount(scalar);
    }

    /// <summary>
    /// 把标量结果解释为计数值：null / DBNull 视为 0。
    /// </summary>
    /// <exception cref="InvalidCastException">当标量值不是数值时抛出。</exception>
    private static int ToCount(object? scalar)
    {
        // 无行或 NULL 是「没有」而不是错误，不能让严格转换抛异常
        if (scalar is null || scalar is DBNull)
        {
            return 0;
        }

        int? value = scalar.ToIntOrNull();
        if (value.HasValue)
        {
            return value.Value;
        }

        throw new InvalidCastException(
            $"The query returned a non-numeric scalar value of type '{scalar.GetType().Name}'. " +
            "Use a COUNT-style query, or call HasRows to test for the presence of rows.");
    }

    #endregion

    #region 分批查询

    /// <summary>
    ///     把过长的取值列表拆成多个批次查询，使用参数化查询防止SQL注入。
    /// </summary>
    /// <param name="sql">SQL查询语句，使用 <c>{0}</c> 作为参数占位符，例如 <c>SELECT * FROM T WHERE Id IN ({0})</c></param>
    /// <param name="parameters">参数值列表</param>
    /// <param name="batchSize">每批次数量(&gt;0)，默认 1000</param>
    /// <returns>合并后的查询结果</returns>
    public virtual DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        DataTable? result = null;

        foreach ((var offset, var length) in EnumerateBatches(parameters.Count, batchSize))
        {
            (var formattedSql, DbParameter[] dbParams) = BuildBatch(sql, parameters, offset, length);
            DataTable batchTable = QueryTable(formattedSql, dbParams);
            result = MergeBatch(result, batchTable);
        }

        return result ?? new DataTable { Locale = CultureInfo.InvariantCulture };
    }

    /// <summary>
    /// 按索引切分批次，避免 Skip/Take 在 List 上的 O(n²) 遍历。
    /// </summary>
    private static IEnumerable<(int Offset, int Length)> EnumerateBatches(int totalCount, int batchSize)
    {
        for (var offset = 0; offset < totalCount; offset += batchSize)
        {
            yield return (offset, Math.Min(batchSize, totalCount - offset));
        }
    }

    /// <summary>
    /// 构造一个批次的 SQL 与参数数组。
    /// </summary>
    private (string Sql, DbParameter[] Parameters) BuildBatch(string sql, List<string> values, int offset, int length)
    {
        var names = new string[length];
        var dbParams = new DbParameter[length];

        for (var i = 0; i < length; i++)
        {
            var name = GetParameterName(i);
            names[i] = name;
            dbParams[i] = CreateParameter(name, (object?)values[offset + i] ?? DBNull.Value);
        }

        var formattedSql = string.Format(CultureInfo.InvariantCulture, sql, string.Join(",", names));
        return (formattedSql, dbParams);
    }

    /// <summary>
    /// 把批次结果合并进累计表；首批决定架构。
    /// </summary>
    private static DataTable MergeBatch(DataTable? accumulated, DataTable batchTable)
    {
        if (accumulated is null)
        {
            return batchTable;
        }

        foreach (DataRow row in batchTable.Rows)
        {
            accumulated.ImportRow(row);
        }

        return accumulated;
    }

    /// <summary>
    ///     获取参数名称（不同数据库使用不同的参数前缀）
    /// </summary>
    /// <param name="index">参数索引</param>
    protected virtual string GetParameterName(int index)
    {
        // 默认使用 @ 前缀（SQL Server / SQLite 兼容）
        return $"@param{index}";
    }

    /// <summary>
    ///     创建参数对象。
    /// </summary>
    /// <param name="parameterName">参数名</param>
    /// <param name="value">参数值</param>
    /// <exception cref="InvalidOperationException">工厂不支持创建参数时抛出。</exception>
    protected virtual DbParameter CreateParameter(string parameterName, object value)
    {
        DbParameter parameter = Factory.CreateParameter()
            ?? throw new InvalidOperationException(
                $"'{Factory.GetType().FullName}' did not provide a {nameof(DbParameter)}.");

        parameter.ParameterName = parameterName;
        parameter.Value = value;
        return parameter;
    }

    #endregion
}
