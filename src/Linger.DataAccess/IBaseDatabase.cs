using System.Data;
using System.Data.Common;

namespace Linger.DataAccess;

public interface IBaseDatabase : IDisposable
{
    bool InTransaction { get; set; }
    DbTransaction BeginTrans();
    void Commit();
    void Rollback();
    void Close();

    int ExecuteNonQuery(CommandType cmdType, string cmdText);
    int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    int ExecuteNonQuery(DbTransaction isOpenTrans, CommandType cmdType, string cmdText);

    int ExecuteNonQuery(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    IDataReader ExecuteReader(CommandType cmdType, string cmdText);
    IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    IDataReader ExecuteReader(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(CommandType cmdType, string cmdText);
    object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(DbConnection conn, DbTransaction isOpenTrans, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    DataSet GetDataSet(CommandType cmdType, string cmdText);
    DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText);

    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<int> ExecuteNonQueryAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText);

    Task<int> ExecuteNonQueryAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText);
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<IDataReader> ExecuteReaderAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText);

    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<object?> ExecuteScalarAsync(DbConnection conn, DbTransaction isOpenTrans, CommandType cmdType, string cmdText);

    Task<object?> ExecuteScalarAsync(DbTransaction isOpenTrans, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText);
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
}
