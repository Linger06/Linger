using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Linger.DataAccess;

/// <summary>
///     操作数据库基类 接口
/// </summary>
public interface IDatabase : IDisposable
{
    bool BulkInsert(DataTable dt);

    int ExecuteBySql(string strSql);
    int ExecuteBySql(StringBuilder strSql);
    int ExecuteBySql(StringBuilder strSql, DbTransaction isOpenTrans);
    int ExecuteBySql(StringBuilder strSql, DbParameter[] parameters);
    int ExecuteBySql(StringBuilder strSql, DbParameter[] parameters, DbTransaction isOpenTrans);

    int ExecuteByProc(string procName);
    int ExecuteByProc(string procName, DbTransaction isOpenTrans);
    int ExecuteByProc(string procName, DbParameter[] parameters);
    int ExecuteByProc(string procName, DbParameter[] parameters, DbTransaction isOpenTrans);

    List<T> FindListBySql<T>(string strSql);
    List<T> FindListBySql<T>(string strSql, DbParameter[] parameters);

    DataTable FindTableBySql(string strSql);
    DataTable FindTableBySql(string strSql, DbParameter[] parameters);
    DataTable FindTableByProc(string procName);
    DataTable FindTableByProc(string procName, DbParameter[] parameters);

    DataSet FindDataSetBySql(string strSql);
    DataSet FindDataSetBySql(string strSql, DbParameter[] parameters);
    DataSet FindDataSetByProc(string procName);
    DataSet FindDataSetByProc(string procName, DbParameter[] parameters);

    T FindEntityBySql<T>(string strSql);
    T FindEntityBySql<T>(string strSql, DbParameter[] parameters);

    Hashtable FindHashtableBySql(string strSql);
    Hashtable FindHashtableBySql(string strSql, DbParameter[] parameters);

    int FindCountBySql(string strSql);
    Task<int> FindCountBySqlAsync(string strSql);
    int FindCountBySql(string strSql, DbParameter[] parameters);

    object? FindMaxBySql(string strSql);
    object? FindMaxBySql(string strSql, DbParameter[] parameters);
}