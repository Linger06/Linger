using System.Data;
using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
///     面向业务的数据库操作契约。
/// </summary>
/// <remarks>
/// 继承 <see cref="IBaseDatabase"/>，因此同一实例既能开启事务、又能把该事务传给带事务参数的重载；
/// 不带事务参数的方法在 <see cref="IBaseDatabase.BeginTrans"/> 之后会自动加入环境事务。
/// 批量插入等能力性 API 不在本接口中，请检测 <see cref="IBulkInsert"/>。
/// </remarks>
public interface IDatabase : IBaseDatabase
{
    #region 通用查询

    /// <summary>执行查询并返回 <see cref="DataSet"/>（含全部结果集）。</summary>
    /// <remarks>
    /// DataSet / DataTable 查询只有同步版本：BCL 的填充 API 全为同步，
    /// 异步流式读取请直接使用数据库 Provider 的 ADO.NET API。存储过程版本见 <see cref="FindDataSetByProc"/>。
    /// </remarks>
    DataSet Query(string sql, params DbParameter[] parameters);

    /// <summary>执行查询并返回首个结果集的 <see cref="DataTable"/>。</summary>
    /// <remarks>存储过程版本见 <see cref="FindTableByProc"/>。</remarks>
    DataTable QueryTable(string sql, params DbParameter[] parameters);

    #endregion

    #region 批量事务执行

    /// <summary>在单个事务中依次执行多条参数化 SQL：全部成功则提交，任一条失败则整体回滚并抛出原异常。</summary>
    /// <remarks>自带连接与事务；处于环境事务中时抛 <see cref="InvalidOperationException"/>，避免与之死锁。</remarks>
    int[] ExecuteTransaction(IEnumerable<SqlStatement> statements);

    /// <summary>在单个事务中依次执行多条参数化 SQL（异步）：全部成功则提交，任一条失败则整体回滚并抛出原异常。</summary>
    /// <inheritdoc cref="ExecuteTransaction(IEnumerable{SqlStatement})" path="/remarks"/>
    Task<int[]> ExecuteTransactionAsync(IEnumerable<SqlStatement> statements,
        CancellationToken cancellationToken = default);

    #endregion

    #region 存在性检查

    /// <summary>判断查询是否返回任何行，不解释列值。</summary>
    bool HasRows(string sql, params DbParameter[] parameters);

    /// <summary>判断查询是否返回任何行（异步），不解释列值。</summary>
    Task<bool> HasRowsAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    #endregion

    #region 执行SQL语句与存储过程

    /// <summary>执行SQL语句，返回受影响行数。</summary>
    int ExecuteBySql(string sql, params DbParameter[] parameters);

    /// <summary>执行SQL语句（异步），返回受影响行数。</summary>
    Task<int> ExecuteBySqlAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>在指定事务中执行SQL语句。</summary>
    int ExecuteBySql(string sql, DbTransaction transaction, params DbParameter[] parameters);

    /// <summary>在指定事务中执行SQL语句（异步）。</summary>
    Task<int> ExecuteBySqlAsync(string sql, DbTransaction transaction, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>执行存储过程，返回受影响行数。</summary>
    int ExecuteByProc(string procName, params DbParameter[] parameters);

    /// <summary>执行存储过程（异步），返回受影响行数。</summary>
    Task<int> ExecuteByProcAsync(string procName, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>在指定事务中执行存储过程。</summary>
    int ExecuteByProc(string procName, DbTransaction transaction, params DbParameter[] parameters);

    #endregion

    #region 查询实体与列表

    /// <summary>查询数据列表、按同名属性映射并返回 List。</summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    List<T> FindListBySql<T>(string sql, params DbParameter[] parameters);

    /// <summary>查询数据列表、使用指定映射函数返回 List。</summary>
    List<T> FindListBySql<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] parameters);

    /// <summary>异步查询数据列表、按同名属性映射并返回 List。</summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    Task<List<T>> FindListBySqlAsync<T>(string sql, DbParameter[]? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>异步查询数据列表、使用指定映射函数返回 List。</summary>
    Task<List<T>> FindListBySqlAsync<T>(string sql, Func<IDataRecord, T> map,
        DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>查询对象、按同名属性映射并返回实体。</summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    T? FindEntityBySql<T>(string sql, params DbParameter[] parameters);

    /// <summary>查询对象、使用指定映射函数返回实体。</summary>
    T? FindEntityBySql<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] parameters);

    #endregion

    #region 存储过程查询 DataTable / DataSet

    // SQL 文本版本见上方 Query / QueryTable，此处只有存储过程版本。

    /// <summary>执行存储过程、返回 DataTable。</summary>
    DataTable FindTableByProc(string procName, params DbParameter[] parameters);

    /// <summary>执行存储过程、返回 DataSet。</summary>
    DataSet FindDataSetByProc(string procName, params DbParameter[] parameters);

    #endregion

    #region 计数与分批

    /// <summary>执行计数查询并返回条数；无行或 NULL 时为 0。</summary>
    int FindCountBySql(string sql, params DbParameter[] parameters);

    /// <summary>执行计数查询并返回条数（异步）；无行或 NULL 时为 0。</summary>
    Task<int> FindCountBySqlAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>把过长的取值列表拆成多个批次参数化查询。</summary>
    /// <remarks>
    /// 仅适用于各批结果可以直接拼接的行查询，不支持依赖全局排序、分页、聚合或去重语义的查询。
    /// </remarks>
    DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000);

    #endregion
}
