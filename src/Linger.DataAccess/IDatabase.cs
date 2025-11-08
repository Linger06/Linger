using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Linger.DataAccess;

/// <summary>
///     �������ݿ���� �ӿ�
/// </summary>
public interface IDatabase : IDisposable
{
    bool BulkInsert(DataTable dt);

    int ExecuteBySql(string sql);
    int ExecuteBySql(StringBuilder sql);
    int ExecuteBySql(StringBuilder sql, DbTransaction transaction);
    int ExecuteBySql(StringBuilder sql, DbParameter[] parameters);
    int ExecuteBySql(StringBuilder sql, DbParameter[] parameters, DbTransaction transaction);

    int ExecuteByProc(string procName);
    int ExecuteByProc(string procName, DbTransaction transaction);
    int ExecuteByProc(string procName, DbParameter[] parameters);
    int ExecuteByProc(string procName, DbParameter[] parameters, DbTransaction transaction);

    List<T> FindListBySql<T>(string sql);
    List<T> FindListBySql<T>(string sql, DbParameter[] parameters);

    DataTable FindTableBySql(string sql);
    Task<DataTable> FindTableBySqlAsync(string sql);
    DataTable FindTableBySql(string sql, DbParameter[] parameters);
    DataTable FindTableByProc(string procName);
    DataTable FindTableByProc(string procName, DbParameter[] parameters);

    DataSet FindDataSetBySql(string sql);
    Task<DataSet> FindDataSetBySqlAsync(string sql);
    DataSet FindDataSetBySql(string sql, DbParameter[] parameters);
    Task<DataSet> FindDataSetBySqlAsync(string sql, DbParameter[] parameters);
    DataSet FindDataSetByProc(string procName);
    DataSet FindDataSetByProc(string procName, DbParameter[] parameters);

    T FindEntityBySql<T>(string sql);
    T FindEntityBySql<T>(string sql, DbParameter[] parameters);

    Hashtable FindHashtableBySql(string sql);
    Hashtable FindHashtableBySql(string sql, DbParameter[] parameters);

    int FindCountBySql(string sql);
    Task<int> FindCountBySqlAsync(string sql, CancellationToken cancellationToken = default);
    int FindCountBySql(string sql, DbParameter[] parameters);
    Task<int> FindCountBySqlAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

    object? FindMaxBySql(string sql);
    object? FindMaxBySql(string sql, DbParameter[] parameters);
}