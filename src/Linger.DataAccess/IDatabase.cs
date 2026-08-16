using System.Data;
using System.Data.Common;
using System.Text;

namespace Linger.DataAccess;

/// <summary>
///     Database interface.
/// </summary>
public interface IDatabase : IDisposable
{
    [Obsolete("BulkInsert will be removed in 2.0.0. Use IBulkInsert instead. See the migration guide in the Linger repository for guidance.")]
    bool BulkInsert(DataTable dt);

    int ExecuteBySql(string sql);
    int ExecuteBySql(string sql, DbParameter[] parameters);
    int ExecuteBySql(string sql, DbTransaction transaction);
    [Obsolete("The parameter order of this overload changes in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    int ExecuteBySql(string sql, DbParameter[] parameters, DbTransaction transaction);
    [Obsolete("StringBuilder overloads will be removed in 2.0.0. Use the string overloads instead. See the migration guide in the Linger repository for guidance.")]
    int ExecuteBySql(StringBuilder sql);
    [Obsolete("StringBuilder overloads will be removed in 2.0.0. Use the string overloads instead. See the migration guide in the Linger repository for guidance.")]
    int ExecuteBySql(StringBuilder sql, DbTransaction transaction);
    [Obsolete("StringBuilder overloads will be removed in 2.0.0. Use the string overloads instead. See the migration guide in the Linger repository for guidance.")]
    int ExecuteBySql(StringBuilder sql, DbParameter[] parameters);
    [Obsolete("StringBuilder overloads will be removed in 2.0.0. Use the string overloads instead. See the migration guide in the Linger repository for guidance.")]
    int ExecuteBySql(StringBuilder sql, DbParameter[] parameters, DbTransaction transaction);

    int ExecuteByProc(string procName);
    int ExecuteByProc(string procName, DbTransaction transaction);
    int ExecuteByProc(string procName, DbParameter[] parameters);
    [Obsolete("The parameter order of this overload changes in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    int ExecuteByProc(string procName, DbParameter[] parameters, DbTransaction transaction);

    DataSet Query(string sql, params DbParameter[] parameters);
    DataTable QueryTable(string sql, params DbParameter[] parameters);
    [Obsolete("QueryAsync will be removed in 2.0.0. Use the synchronous Query overload instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> QueryAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);
    [Obsolete("QueryTableAsync will be removed in 2.0.0. Use the synchronous QueryTable overload instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> QueryTableAsync(string sql, DbParameter[]? parameters = null, CancellationToken cancellationToken = default);

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    List<T> FindListBySql<T>(string sql);
    List<T> FindListBySql<T>(string sql, Func<IDataRecord, T> map);
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    List<T> FindListBySql<T>(string sql, DbParameter[] parameters);
    [Obsolete("The parameter order of this overload changes in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    List<T> FindListBySql<T>(string sql, DbParameter[] parameters, Func<IDataRecord, T> map);

    DataTable FindTableBySql(string sql);
    [Obsolete("FindTableBySqlAsync will be removed in 2.0.0. Use QueryTable instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> FindTableBySqlAsync(string sql);
    [Obsolete("FindTableBySqlAsync will be removed in 2.0.0. Use QueryTable instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> FindTableBySqlAsync(string sql, CancellationToken cancellationToken);
    [Obsolete("FindTableBySqlAsync will be removed in 2.0.0. Use QueryTable instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> FindTableBySqlAsync(string sql, DbParameter[] parameters);
    [Obsolete("FindTableBySqlAsync will be removed in 2.0.0. Use QueryTable instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> FindTableBySqlAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken);
    DataTable FindTableBySql(string sql, DbParameter[] parameters);
    DataTable FindTableByProc(string procName);
    DataTable FindTableByProc(string procName, DbParameter[] parameters);

    [Obsolete("FindDataSetBySql will be removed in 2.0.0. Use Query instead. See the migration guide in the Linger repository for guidance.")]
    DataSet FindDataSetBySql(string sql);
    [Obsolete("FindDataSetBySqlAsync will be removed in 2.0.0. Use Query instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> FindDataSetBySqlAsync(string sql);
    [Obsolete("FindDataSetBySql will be removed in 2.0.0. Use Query instead. See the migration guide in the Linger repository for guidance.")]
    DataSet FindDataSetBySql(string sql, DbParameter[] parameters);
    [Obsolete("FindDataSetBySqlAsync will be removed in 2.0.0. Use Query instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataSet> FindDataSetBySqlAsync(string sql, DbParameter[] parameters);
    DataSet FindDataSetByProc(string procName);
    DataSet FindDataSetByProc(string procName, DbParameter[] parameters);

    DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000);
    [Obsolete("QueryInBatchesAsync will be removed in 2.0.0. Use the synchronous QueryInBatches instead. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> QueryInBatchesAsync(string sql, List<string> parameters, int batchSize = 1000, CancellationToken cancellationToken = default);
    DataTable QueryInBatchesRaw(string sql, List<string> values, int batchSize = 1000, bool quote = true);
    [Obsolete("QueryInBatchesRawAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<DataTable> QueryInBatchesRawAsync(string sql, List<string> values, int batchSize = 1000, bool quote = true, CancellationToken cancellationToken = default);

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    T? FindEntityBySql<T>(string sql);
    T? FindEntityBySql<T>(string sql, Func<IDataRecord, T> map);
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map records. Use the mapper overload for AOT/trimming scenarios.")]
#endif
    T? FindEntityBySql<T>(string sql, DbParameter[] parameters);
    [Obsolete("The parameter order of this overload changes in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    T? FindEntityBySql<T>(string sql, DbParameter[] parameters, Func<IDataRecord, T> map);

    int FindCountBySql(string sql);
    [Obsolete("This overload will be removed in 2.0.0. Pass DbParameter[]? parameters before the cancellation token. See the migration guide in the Linger repository for guidance.")]
    Task<int> FindCountBySqlAsync(string sql, CancellationToken cancellationToken = default);
    int FindCountBySql(string sql, DbParameter[] parameters);
    Task<int> FindCountBySqlAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

    [Obsolete("FindMaxBySql will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    object? FindMaxBySql(string sql);
    [Obsolete("FindMaxBySql will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    object? FindMaxBySql(string sql, DbParameter[] parameters);
    [Obsolete("FindMaxBySqlAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<object?> FindMaxBySqlAsync(string sql);
    [Obsolete("FindMaxBySqlAsync will be removed in 2.0.0. See the migration guide in the Linger repository for guidance.")]
    Task<object?> FindMaxBySqlAsync(string sql, DbParameter[] parameters);
}
