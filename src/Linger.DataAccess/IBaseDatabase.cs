using System.Data;
using System.Data.Common;

namespace Linger.DataAccess;

public interface IBaseDatabase : IDisposable
{
    bool InTransaction { get; }
    DbTransaction BeginTrans();
    void Commit();
    void Rollback();
    void Close();

    int ExecuteNonQuery(CommandType cmdType, string cmdText);
    int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText);
    int ExecuteNonQuery(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText);

    int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    IDataReader ExecuteReader(CommandType cmdType, string cmdText);
    IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    IDataReader ExecuteReader(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(CommandType cmdType, string cmdText);
    object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    object? ExecuteScalar(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText);

    object? ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);

    DataSet GetDataSet(CommandType cmdType, string cmdText);
    DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameters);

    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<IDataReader> ExecuteReaderAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<IDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbConnection connection, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<object?> ExecuteScalarAsync(DbConnection conn, DbTransaction transaction, CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, CancellationToken cancellationToken = default);
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, params DbParameter[] parameters);
    Task<DataSet> GetDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] parameters, CancellationToken cancellationToken);

}
