using System.Data;

namespace Linger.Dapper;

public interface IDapperHelper
{
    bool Delete<T>(T entity) where T : class;
    bool DeleteAll<T>() where T : class;
    Task<bool> DeleteAllAsync<T>() where T : class;
    Task<bool> DeleteAsync<T>(T entity) where T : class;

    int Execute(string sql, object? param = null, Func<int, IDbTransaction, int>? successFunc = null,
        Func<Exception, IDbTransaction, int>? failFunc = null, int? commandTimeout = null,
        CommandType? commandType = null);

    int ExecuteProc(string procName, object? param = null, Func<int, IDbTransaction, int>? successFunc = null,
        Func<Exception, IDbTransaction, int>? failFunc = null, int? commandTimeout = null);

    List<T> ExecuteProc<T>(string proc, object? param);
    IDataReader ExecuteReader(string sql, object? param);
    object? ExecuteScalar(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);
    T? ExecuteScalar<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);
    T? ExecuteScalarForT<T>(string sql, object? param);
    int ExecuteTransaction(Dictionary<string, object> dic);
    int ExecuteTransaction(string[] sqlArray);
    T Get<T>(string id) where T : class;
    List<T> GetAll<T>() where T : class;
    Task<List<T>> GetAllAsync<T>() where T : class;
    long Insert<T>(T entity) where T : class;
    Task<long> InsertAsync<T>(T entity) where T : class;
    dynamic Query(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);

    IEnumerable<T> Query<T>(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null);

    IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,
        object? param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

    DataSet QueryDataSet(string sql, object? param = null, IEnumerable<string>? tableNames = null,
        int? commandTimeout = null, CommandType? commandType = null);

    DataSet QueryDataSetProc(string procName, object? param = null, IEnumerable<string>? tableNames = null,
        int? commandTimeout = null);

    DataTable QueryDataTable(string sql, object? param = null, string? tableName = null, int? commandTimeout = null,
        CommandType? commandType = null);

    DataTable QueryDataTableProc(string procName, object? param = null, string? tableName = null,
        int? commandTimeout = null);

    T QueryFirst<T>(string sql, object? param = null);

    dynamic? QueryFirstOrDefault(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null);

    T? QueryFirstOrDefault<T>(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null);

    void QueryMultiple<T1, T2, T3, T4, T5, T6>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>>
            action, object? param = null, int? commandTimeout = null, CommandType? commandType = null);

    void QueryMultiple<T1, T2, T3, T4, T5>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>> action,
        object? param = null, int? commandTimeout = null, CommandType? commandType = null);

    void QueryMultiple<T1, T2, T3, T4>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> action, object? param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    void QueryMultiple<T1, T2, T3>(string sql, Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> action,
        object? param = null, int? commandTimeout = null, CommandType? commandType = null);

    void QueryMultiple<T1, T2>(string sql, Action<IEnumerable<T1>, IEnumerable<T2>> action, object? param = null,
        int? commandTimeout = null, CommandType? commandType = null);

    dynamic QueryProc(string procName, object? param = null, int? commandTimeout = null);
    IEnumerable<T> QueryProc<T>(string procName, object? param = null, int? commandTimeout = null);
    T QuerySingle<T>(string sql, object? param = null);
    T? QuerySingleOrDefault<T>(string sql, object? param = null);
    DataTable QueryToDataTable(string sql);
    void SetCommandTimeout(int commandTimeout);
    bool Update<T>(T entity) where T : class;
    Task<bool> UpdateAsync<T>(T entity) where T : class;
}