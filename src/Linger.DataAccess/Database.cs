using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

namespace Linger.DataAccess;

/// <summary>
///     操作数据库基类
/// </summary>
public class Database(IProvider provider, string strConnection) : BaseDatabase(provider, strConnection), IDatabase
{

    #region SqlBulkCopy大批量数据插入

    /// <summary>
    ///     大批量数据插入
    /// </summary>
    /// <param name="dt">资料表</param>
    /// <returns></returns>
    public bool BulkInsert(DataTable dt)
    {
        return false;
    }

    #endregion

    #region 执行SQL语句

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public int ExecuteBySql(string strSql)
    {
        return ExecuteNonQuery(CommandType.Text, strSql);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder strSql)
    {
        return ExecuteNonQuery(CommandType.Text, strSql.ToString());
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder strSql, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, strSql.ToString());
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder strSql, DbParameter[] parameters)
    {
        return ExecuteNonQuery(CommandType.Text, strSql.ToString(), parameters);
    }

    /// <summary>
    ///     执行SQL语句
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <param name="isOpenTrans">事务对象</param>
    /// <returns></returns>
    public int ExecuteBySql(StringBuilder strSql, DbParameter[] parameters, DbTransaction isOpenTrans)
    {
        return ExecuteNonQuery(isOpenTrans, CommandType.Text, strSql.ToString(), parameters);
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
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string strSql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql);
        return dr.ReaderToList<T>();
    }

    /// <summary>
    ///     查询数据列表、返回List
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public List<T> FindListBySql<T>(string strSql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql, parameters);
        return dr.ReaderToList<T>();
    }

    #endregion

    #region 查询数据列表、返回DataTable

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public DataTable FindTableBySql(string strSql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql);
        return dr.ReaderToDataTable();
    }

    public async Task<DataTable> FindTableBySqlAsync(string strSql)
    {
        IDataReader dr = await ExecuteReaderAsync(CommandType.Text, strSql).ConfigureAwait(false);
        return dr.ReaderToDataTable();
    }

    /// <summary>
    ///     查询数据列表、返回 DataTable
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataTable FindTableBySql(string strSql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql, parameters);
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
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public DataSet FindDataSetBySql(string strSql)
    {
        return GetDataSet(CommandType.Text, strSql);
    }

    /// <summary>
    ///     查询数据列表、返回DataSet
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public DataSet FindDataSetBySql(string strSql, DbParameter[] parameters)
    {
        return GetDataSet(CommandType.Text, strSql, parameters);
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
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public T FindEntityBySql<T>(string strSql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql);
        return dr.ReaderToModel<T>();
    }

    /// <summary>
    ///     查询对象、返回实体
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public T FindEntityBySql<T>(string strSql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql, parameters);
        return dr.ReaderToModel<T>();
    }

    #endregion

    #region 查询对象、返回哈希表

    /// <summary>
    ///     查询对象、返回哈希表
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public Hashtable FindHashtableBySql(string strSql)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql);
        return dr.ReaderToHashtable();
    }

    /// <summary>
    ///     查询对象、返回哈希表
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public Hashtable FindHashtableBySql(string strSql, DbParameter[] parameters)
    {
        IDataReader dr = ExecuteReader(CommandType.Text, strSql, parameters);
        return dr.ReaderToHashtable();
    }

    #endregion

    #region 查询数据、返回条数

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public int FindCountBySql(string strSql)
    {
        return ExecuteScalar(CommandType.Text, strSql).ToInt();
    }

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public async Task<int> FindCountBySqlAsync(string strSql)
    {
        return (await ExecuteScalarAsync(CommandType.Text, strSql).ConfigureAwait(false)).ToInt();
    }

    /// <summary>
    ///     查询数据、返回条数
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public int FindCountBySql(string strSql, DbParameter[] parameters)
    {
        return ExecuteScalar(CommandType.Text, strSql, parameters).ToInt();
    }

    #endregion

    #region 查询数据、返回最大数

    /// <summary>
    ///     查询数据、返回最大数
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <returns></returns>
    public object? FindMaxBySql(string strSql)
    {
        return ExecuteScalar(CommandType.Text, strSql);
    }

    /// <summary>
    ///     查询数据、返回最大数
    /// </summary>
    /// <param name="strSql">Sql语句</param>
    /// <param name="parameters">sql语句对应参数</param>
    /// <returns></returns>
    public object? FindMaxBySql(string strSql, DbParameter[] parameters)
    {
        return ExecuteScalar(CommandType.Text, strSql, parameters);
    }

    #endregion
}
