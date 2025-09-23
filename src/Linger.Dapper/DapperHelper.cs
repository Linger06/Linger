using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Linger.Dapper;

/// <summary>
///     Dapper帮助类，简化Dapper的使用
/// </summary>
public class DapperHelper<TConnection> : IDapperHelper where TConnection : IDbConnection, new()
{
    /// <summary>
    ///     当前对象的字符串链接
    /// </summary>
    private readonly string _connStr;

    /// <summary>
    ///     当前对象的链接数据库类型
    /// </summary>
    private readonly TConnection _dbConnection;

    /// <summary>
    ///     当前对象的命令超时时间，单位：秒
    /// </summary>
    private int? _mCommandTimeout;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    public DapperHelper(TConnection dbConnection, int? commandTimeout = null)
    {
        _connStr = dbConnection.ConnectionString;
        _mCommandTimeout = commandTimeout;
        _dbConnection = dbConnection;
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="connStr">链接字符串，需与DataBaseType进行配对</param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    public DapperHelper(string connStr, int? commandTimeout = null)
    {
        _connStr = connStr;
        _dbConnection = new TConnection { ConnectionString = _connStr };
        _mCommandTimeout = commandTimeout;
    }

    /// <summary>
    ///     设置命令超时时间
    /// </summary>
    /// <param name="commandTimeout">超时时间，单位：秒</param>
    public void SetCommandTimeout(int commandTimeout)
    {
        _mCommandTimeout = commandTimeout;
    }

    /// <summary>
    ///     获取数据库连接
    /// </summary>
    /// <returns></returns>
    private TConnection GetDbConnection()
    {
        var dbConnection = new TConnection { ConnectionString = _connStr };

        if (dbConnection == null)
        {
            throw new Exception("未指定数据库类型");
        }

        return dbConnection;
    }

    /// <summary>
    ///     获取实例对象
    /// </summary>
    /// <param name="connStr">符合数据库的链接字符串</param>
    /// <param name="commandTimeout">执行命令超时时间，单位：秒</param>
    /// <returns></returns>
    public static DapperHelper<TConnection> GetInstance(string connStr, int? commandTimeout = null)
    {
        return new DapperHelper<TConnection>(connStr, commandTimeout);
    }

    /// <summary>
    ///     将IDataReader转为DataSet，其中表名按照顺序命名为Table1、Table2...
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static DataSet ConvertDataReaderToDataSet(IDataReader reader)
    {
        var dataSet = new DataSet();
        do
        {
            // Create new data table
            DataTable? schemaTable = reader.GetSchemaTable();
            var dataTable = new DataTable();

            if (schemaTable != null)
            {
                // A query returning records was executed

                for (var i = 0; i < schemaTable.Rows.Count; i++)
                {
                    DataRow dataRow = schemaTable.Rows[i];
                    // Create a column name that is unique in the data table
                    var columnName = (string)dataRow["ColumnName"]; //+ " // Add the column definition to the data table
                    var column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                    dataTable.Columns.Add(column);
                }

                dataSet.Tables.Add(dataTable);
                // Fill the data table we just created
                while (reader.Read())
                {
                    DataRow dataRow = dataTable.NewRow();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        dataRow[i] = reader.GetValue(i);
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }
            else
            {
                // No records were returned
                var column = new DataColumn("RowsAffected");
                dataTable.Columns.Add(column);
                dataSet.Tables.Add(dataTable);
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = reader.RecordsAffected;
                dataTable.Rows.Add(dataRow);
            }
        } while (reader.NextResult());

        return dataSet;
    }

    #region 查询

    /// <summary>
    ///     动态类型查询
    /// </summary>
    /// <param name="sql">查询文本（存储过程名称）</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回动态类型枚举对象</returns>
    public dynamic Query(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.Query(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     动态类型存储过程查询
    /// </summary>
    /// <param name="procName">存储过程名称</param>
    /// <param name="param">存储过程参数</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <returns></returns>
    public dynamic QueryProc(string procName, object? param = null, int? commandTimeout = null)
    {
        return Query(procName, param, commandTimeout, CommandType.StoredProcedure);
    }

    /// <summary>
    ///     强类型查询
    /// </summary>
    /// <typeparam name="T">指定返回的类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回指定T类型的集合</returns>
    public IEnumerable<T> Query<T>(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.Query<T>(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     强类型存储过程查询
    /// </summary>
    /// <typeparam name="T">指定返回的类型</typeparam>
    /// <param name="procName">存储过程名称</param>
    /// <param name="param">存储过程参数</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <returns>返回指定T类型的集合</returns>
    public IEnumerable<T> QueryProc<T>(string procName, object? param = null, int? commandTimeout = null)
    {
        return Query<T>(procName, param, commandTimeout, CommandType.StoredProcedure);
    }

    /// <summary>
    ///     连表强类型查询（一对一、一对多）
    /// </summary>
    /// <typeparam name="TFirst">查询得到的类型TFirst</typeparam>
    /// <typeparam name="TSecond">查询得到的类型TSecond</typeparam>
    /// <typeparam name="TReturn">返回的类型TReturn</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="map">关联TFirst和TSecond的映射关系函数</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="splitOn">分割字段字符串</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回指定TReturn类型的集合</returns>
    public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,
        object? param = null, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.Query(sql, map, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType, splitOn: splitOn).Distinct();
    }

    /// <summary>
    ///     动态类型查询，返回第一条数据，如果没有，则返回null
    /// </summary>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回一条动态类型数据</returns>
    public dynamic? QueryFirstOrDefault(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.QueryFirstOrDefault(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     强类型查询，返回第一条数据，如果没有，则返回default(T)
    /// </summary>
    /// <typeparam name="T">指定返回的类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回一条T类型数据</returns>
    public T? QueryFirstOrDefault<T>(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.QueryFirstOrDefault<T>(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     多表查询，需要在action中接收结果
    /// </summary>
    /// <typeparam name="T1">第一个查询结果转换类型</typeparam>
    /// <typeparam name="T2">第二个查询结果转换类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="action">获取转换类型的回调函数，需要在此接收结果</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    public void QueryMultiple<T1, T2>(string sql, Action<IEnumerable<T1>, IEnumerable<T2>> action, object? param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        using SqlMapper.GridReader multiReader = connection.QueryMultiple(sql);
        IEnumerable<T1> t1 = multiReader.Read<T1>();
        IEnumerable<T2> t2 = multiReader.Read<T2>();
        action(t1, t2);
    }

    /// <summary>
    ///     多表查询，需要在action中接收结果
    /// </summary>
    /// <typeparam name="T1">第一个查询结果转换类型</typeparam>
    /// <typeparam name="T2">第二个查询结果转换类型</typeparam>
    /// <typeparam name="T3">第三个查询结果转换类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="action">获取转换类型的回调函数，需要在此接收结果</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    public void QueryMultiple<T1, T2, T3>(string sql, Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> action,
        object? param = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        using SqlMapper.GridReader multiReader = connection.QueryMultiple(sql);
        IEnumerable<T1> t1 = multiReader.Read<T1>();
        IEnumerable<T2> t2 = multiReader.Read<T2>();
        IEnumerable<T3> t3 = multiReader.Read<T3>();
        action(t1, t2, t3);
    }

    /// <summary>
    ///     多表查询，需要在action中接收结果
    /// </summary>
    /// <typeparam name="T1">第一个查询结果转换类型</typeparam>
    /// <typeparam name="T2">第二个查询结果转换类型</typeparam>
    /// <typeparam name="T3">第三个查询结果转换类型</typeparam>
    /// <typeparam name="T4">第四个查询结果转换类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="action">获取转换类型的回调函数，需要在此接收结果</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    public void QueryMultiple<T1, T2, T3, T4>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> action, object? param = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        using SqlMapper.GridReader multiReader = connection.QueryMultiple(sql);
        IEnumerable<T1> t1 = multiReader.Read<T1>();
        IEnumerable<T2> t2 = multiReader.Read<T2>();
        IEnumerable<T3> t3 = multiReader.Read<T3>();
        IEnumerable<T4> t4 = multiReader.Read<T4>();
        action(t1, t2, t3, t4);
    }

    /// <summary>
    ///     多表查询，需要在action中接收结果
    /// </summary>
    /// <typeparam name="T1">第一个查询结果转换类型</typeparam>
    /// <typeparam name="T2">第二个查询结果转换类型</typeparam>
    /// <typeparam name="T3">第三个查询结果转换类型</typeparam>
    /// <typeparam name="T4">第四个查询结果转换类型</typeparam>
    /// <typeparam name="T5">第五个查询结果转换类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="action">获取转换类型的回调函数，需要在此接收结果</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    public void QueryMultiple<T1, T2, T3, T4, T5>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>> action,
        object? param = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        using SqlMapper.GridReader multiReader = connection.QueryMultiple(sql);
        IEnumerable<T1> t1 = multiReader.Read<T1>();
        IEnumerable<T2> t2 = multiReader.Read<T2>();
        IEnumerable<T3> t3 = multiReader.Read<T3>();
        IEnumerable<T4> t4 = multiReader.Read<T4>();
        IEnumerable<T5> t5 = multiReader.Read<T5>();
        action(t1, t2, t3, t4, t5);
    }

    /// <summary>
    ///     多表查询，需要在action中接收结果
    /// </summary>
    /// <typeparam name="T1">第一个查询结果转换类型</typeparam>
    /// <typeparam name="T2">第二个查询结果转换类型</typeparam>
    /// <typeparam name="T3">第三个查询结果转换类型</typeparam>
    /// <typeparam name="T4">第四个查询结果转换类型</typeparam>
    /// <typeparam name="T5">第五个查询结果转换类型</typeparam>
    /// <typeparam name="T6">第六个查询结果转换类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="action">获取转换类型的回调函数，需要在此接收结果</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    public void QueryMultiple<T1, T2, T3, T4, T5, T6>(string sql,
        Action<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>>
            action, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        using SqlMapper.GridReader multiReader = connection.QueryMultiple(sql);
        IEnumerable<T1> t1 = multiReader.Read<T1>();
        IEnumerable<T2> t2 = multiReader.Read<T2>();
        IEnumerable<T3> t3 = multiReader.Read<T3>();
        IEnumerable<T4> t4 = multiReader.Read<T4>();
        IEnumerable<T5> t5 = multiReader.Read<T5>();
        IEnumerable<T6> t6 = multiReader.Read<T6>();
        action(t1, t2, t3, t4, t5, t6);
    }

    /// <summary>
    ///     强类型查询第一行第一列结果
    /// </summary>
    /// <typeparam name="T">指定返回的结果类型</typeparam>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns>返回T类型的第一行第一列结果</returns>
    public T? ExecuteScalar<T>(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.ExecuteScalar<T>(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     查询第一行第一个结果
    /// </summary>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns></returns>
    public object? ExecuteScalar(string sql, object? param = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        return connection.ExecuteScalar(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
    }

    /// <summary>
    ///     查询返回表
    /// </summary>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="tableName">指定返回结果的表名，如果不指定则返回空表名</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns></returns>
    public DataTable QueryDataTable(string sql, object? param = null, string? tableName = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        DataTable table = string.IsNullOrWhiteSpace(tableName) ? new DataTable() : new DataTable(tableName);

        IDataReader reader = connection.ExecuteReader(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
        table.Load(reader);
        return table;
    }

    /// <summary>
    ///     查询存储过程返回表
    /// </summary>
    /// <param name="procName">存储过程名称</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="tableName">指定返回结果的表名，如果不指定则返回空表名</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <returns></returns>
    public DataTable QueryDataTableProc(string procName, object? param = null, string? tableName = null,
        int? commandTimeout = null)
    {
        return QueryDataTable(procName, param, tableName, commandTimeout, CommandType.StoredProcedure);
    }

    /// <summary>
    ///     查询返回表集
    /// </summary>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="tableNames">按顺序为返回表起别名，如果不指定，返回的表名将命名为Table1、Table2...</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns></returns>
    public DataSet QueryDataSet(string sql, object? param = null, IEnumerable<string>? tableNames = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        using IDbConnection connection = GetDbConnection();
        var tableSet = new DataSet();
        IDataReader reader = connection.ExecuteReader(sql, param, commandTimeout: commandTimeout ?? _mCommandTimeout,
            commandType: commandType);
        if (tableNames == null)
        {
            tableSet = ConvertDataReaderToDataSet(reader);
        }
        else
        {
            tableSet.Load(reader, LoadOption.OverwriteChanges, tableNames.ToArray());
        }

        return tableSet;
    }

    /// <summary>
    ///     查询存储过程返回表集
    /// </summary>
    /// <param name="procName">存储过程名称</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="tableNames">按顺序为返回表起别名，如果不指定，返回的表名将命名为Table1、Table2...</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <returns></returns>
    public DataSet QueryDataSetProc(string procName, object? param = null, IEnumerable<string>? tableNames = null,
        int? commandTimeout = null)
    {
        return QueryDataSet(procName, param, tableNames, commandTimeout, CommandType.StoredProcedure);
    }

    #endregion 查询

    #region 增删改

    /// <summary>
    ///     执行(带事务)
    /// </summary>
    /// <param name="sql">执行文本</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="successFunc">执行成功回调函数（不传入则成功后自动提交），返回执行成功行数和事务对象，需要自己对事务进行提交或回滚，否则数据库会一直等待</param>
    /// <param name="failFunc">执行失败回调函数（不传入则失败后自动回滚），返回执行失败时的异常和事务对象，需要自己对事务进行提交或回滚，否则数据库会一直等待</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <param name="commandType">执行类型，对应sql使用</param>
    /// <returns></returns>
    public int Execute(string sql, object? param = null, Func<int, IDbTransaction, int>? successFunc = null,
        Func<Exception, IDbTransaction, int>? failFunc = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        int affectedRows;
        using IDbConnection connection = GetDbConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            affectedRows = connection.Execute(sql, param, transaction, commandTimeout ?? _mCommandTimeout, commandType);
            if (successFunc != null)
            {
                affectedRows = successFunc(affectedRows, transaction);
            }
            else
            {
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            if (failFunc != null)
            {
                affectedRows = failFunc(ex, transaction);
            }
            else
            {
                transaction.Rollback();
                throw;
            }
        }

        return affectedRows;
    }

    /// <summary>
    ///     执行存储过程(带事务)
    /// </summary>
    /// <param name="procName">存储过程名称</param>
    /// <param name="param">参数化查询条件对象</param>
    /// <param name="successFunc">执行成功回调函数（不传入则成功后自动提交），返回执行成功行数和事务对象，需要自己对事务进行提交或回滚，否则数据库会一直等待</param>
    /// <param name="failFunc">执行失败回调函数（不传入则失败后自动回滚），返回执行失败时的异常和事务对象，需要自己对事务进行提交或回滚，否则数据库会一直等待</param>
    /// <param name="commandTimeout">执行超时时间，单位：秒。仅当前命令生效</param>
    /// <returns></returns>
    public int ExecuteProc(string procName, object? param = null, Func<int, IDbTransaction, int>? successFunc = null,
        Func<Exception, IDbTransaction, int>? failFunc = null, int? commandTimeout = null)
    {
        return Execute(procName, param, successFunc, failFunc, commandTimeout, CommandType.StoredProcedure);
    }

    #endregion 增删改

    #region 后续添加的 ---------------------------------------------------------------

    /// <summary>
    ///     查询列表（返回DataTable）
    /// </summary>
    /// <returns></returns>
    public DataTable QueryToDataTable(string sql)
    {
        var table = new DataTable("MyTable");
        using IDbConnection con = GetDbConnection();
        IDataReader reader = con.ExecuteReader(sql);
        table.Load(reader);
        return table;
    }

    ///// <summary>
    /////     查询列表
    ///// </summary>
    ///// <param name="sql">查询的sql</param>
    ///// <param name="param">替换参数</param>
    ///// <returns></returns>
    //public List<T> Query<T>(string sql, object? param = null)
    //{
    //    using IDbConnection con = _dbConnection;
    //    return con.Query<T>(sql, param).ToList();
    //}

    /// <summary>
    ///     查询第一个数据
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public T QueryFirst<T>(string sql, object? param = null)
    {
        using IDbConnection con = GetDbConnection();
        return con.Query<T>(sql, param).ToList().First();
    }

    /// <summary>
    ///     查询单条数据
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public T QuerySingle<T>(string sql, object? param = null)
    {
        using IDbConnection con = GetDbConnection();
        return con.Query<T>(sql, param).ToList().Single();
    }

    /// <summary>
    ///     查询单条数据没有返回默认值
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public T? QuerySingleOrDefault<T>(string sql, object? param = null)
    {
        using IDbConnection con = GetDbConnection();

        var result = con.Query<T>(sql, param).ToList();

        return result.SingleOrDefault();
    }

    /// <summary>
    ///     Reader获取数据
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public IDataReader ExecuteReader(string sql, object? param)
    {
        using IDbConnection con = GetDbConnection();
        return con.ExecuteReader(sql, param);
    }

    /// <summary>
    ///     Scalar获取数据
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public T? ExecuteScalarForT<T>(string sql, object? param)
    {
        using IDbConnection con = GetDbConnection();
        return con.ExecuteScalar<T>(sql, param);
    }

    /// <summary>
    ///     带参数的存储过程
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public List<T> ExecuteProc<T>(string proc, object? param)
    {
        using IDbConnection con = GetDbConnection();
        var list = con.Query<T>(proc, param, null, true, null, CommandType.StoredProcedure).ToList();
        return list;
    }

    /// <summary>
    ///     事务1 - 全SQL
    /// </summary>
    /// <param name="sqlArray">多条SQL</param>
    /// <returns></returns>
    public int ExecuteTransaction(string[] sqlArray)
    {
        using IDbConnection con = GetDbConnection();
        using IDbTransaction transaction = con.BeginTransaction();
        try
        {
            var result = 0;
            foreach (var sql in sqlArray)
            {
                result += con.Execute(sql, null, transaction);
            }

            transaction.Commit();
            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return 0;
        }
    }

    /// <summary>
    ///     事务2 - 声明参数
    ///     demo:
    ///     dic.Add("Insert into Users values (@UserName, @Email, @Address)",
    ///     new { UserName = "jack", Email = "380234234@qq.com", Address = "上海" });
    /// </summary>
    /// <param name="dic">多条SQL</param>
    /// <returns></returns>
    public int ExecuteTransaction(Dictionary<string, object> dic)
    {
        using IDbConnection con = GetDbConnection();
        using IDbTransaction transaction = con.BeginTransaction();
        try
        {
            var result = 0;
            foreach (KeyValuePair<string, object> sql in dic)
            {
                result += con.Execute(sql.Key, sql.Value, transaction);
            }

            transaction.Commit();
            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return 0;
        }
    }

    #endregion 后续添加的 ---------------------------------------------------------------

    #region Dapper.Contrib

    public List<T> GetAll<T>() where T : class
    {
        return _dbConnection.GetAll<T>().ToList();
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class
    {
        return (await _dbConnection.GetAllAsync<T>()).ToList();
    }

    public T Get<T>(string id) where T : class
    {
        return _dbConnection.Get<T>(id);
    }

    public bool Update<T>(T entity) where T : class
    {
        return _dbConnection.Update(entity);
    }

    public async Task<bool> UpdateAsync<T>(T entity) where T : class
    {
        return await _dbConnection.UpdateAsync(entity);
    }

    public long Insert<T>(T entity) where T : class
    {
        return _dbConnection.Insert(entity);
    }

    public async Task<long> InsertAsync<T>(T entity) where T : class
    {
        return await _dbConnection.InsertAsync(entity);
    }

    public bool Delete<T>(T entity) where T : class
    {
        return _dbConnection.Delete(entity);
    }

    public async Task<bool> DeleteAsync<T>(T entity) where T : class
    {
        return await _dbConnection.DeleteAsync(entity);
    }

    public bool DeleteAll<T>() where T : class
    {
        return _dbConnection.DeleteAll<T>();
    }

    public async Task<bool> DeleteAllAsync<T>() where T : class
    {
        return await _dbConnection.DeleteAllAsync<T>();
    }

    #endregion Dapper.Contrib
}