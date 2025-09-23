#if NET7_0_OR_GREATER
using System.Data;
using System.Text;
using Dapper;

namespace Linger.Dapper;

public static class ConnectionExtensions
{
    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>Multiple result sets</returns>
    public static IEnumerable<IEnumerable<object>> QueryMultiple(this IDbConnection cnn, IEnumerable<string> sqlList,
        object? param = null, CommandType? commandType = null)
    {
        var inputList = (List<string>)sqlList;
        using SqlMapper.GridReader reader = cnn.QueryMultiple(inputList.Concat(), param, commandType: commandType);
        var results = new IEnumerable<object>[inputList.Count];
        for (var i = 0; i < inputList.Count; i++)
        {
            results[i] = reader.Read();
        }

        return results;
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>Multiple result sets</returns>
    public static async Task<IEnumerable<IEnumerable<object>>> QueryMultipleAsync(this IDbConnection cnn,
        IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        var inputList = (List<string>)sqlList;
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(inputList.Concat(), param, commandType: commandType);
        var results = new IEnumerable<object>[inputList.Count];
        for (var i = 0; i < inputList.Count; i++)
        {
            results[i] = await reader.ReadAsync();
        }

        return results;
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2) QueryMultiple<T1, T2>(this IDbConnection cnn,
        IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();

        return (result1, result2);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2)> QueryMultipleAsync<T1, T2>(
        this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();

        return (result1, result2);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3) QueryMultiple<T1, T2, T3>(
        this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();

        return (result1, result2, result3);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3)>
        QueryMultipleAsync<T1, T2, T3>(this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null,
            CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();

        return (result1, result2, result3);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4)
        QueryMultiple<T1, T2, T3, T4>(this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null,
            CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();

        return (result1, result2, result3, result4);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4)>
        QueryMultipleAsync<T1, T2, T3, T4>(this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null,
            CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();

        return (result1, result2, result3, result4);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5) QueryMultiple<T1, T2, T3, T4, T5>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();

        return (result1, result2, result3, result4, result5);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5)> QueryMultipleAsync<T1, T2, T3, T4, T5>(this IDbConnection cnn,
            IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();

        return (result1, result2, result3, result4, result5);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5, IEnumerable<T6> Result6) QueryMultiple<T1, T2, T3, T4, T5, T6>(this IDbConnection cnn,
            IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();
        IEnumerable<T6> result6 = reader.Read<T6>();

        return (result1, result2, result3, result4, result5, result6);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5, IEnumerable<T6> Result6)> QueryMultipleAsync<T1, T2, T3, T4, T5, T6>(
            this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();
        IEnumerable<T6> result6 = await reader.ReadAsync<T6>();

        return (result1, result2, result3, result4, result5, result6);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7)
        QueryMultiple<T1, T2, T3, T4, T5, T6, T7>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();
        IEnumerable<T6> result6 = reader.Read<T6>();
        IEnumerable<T7> result7 = reader.Read<T7>();

        return (result1, result2, result3, result4, result5, result6, result7);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7)>
        QueryMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();
        IEnumerable<T6> result6 = await reader.ReadAsync<T6>();
        IEnumerable<T7> result7 = await reader.ReadAsync<T7>();

        return (result1, result2, result3, result4, result5, result6, result7);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8)
        QueryMultiple<T1, T2, T3, T4, T5, T6, T7, T8>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();
        IEnumerable<T6> result6 = reader.Read<T6>();
        IEnumerable<T7> result7 = reader.Read<T7>();
        IEnumerable<T8> result8 = reader.Read<T8>();

        return (result1, result2, result3, result4, result5, result6, result7, result8);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8)>
        QueryMultipleAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();
        IEnumerable<T6> result6 = await reader.ReadAsync<T6>();
        IEnumerable<T7> result7 = await reader.ReadAsync<T7>();
        IEnumerable<T8> result8 = await reader.ReadAsync<T8>();

        return (result1, result2, result3, result4, result5, result6, result7, result8);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <typeparam name="T9">The type of the ninth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8,
        IEnumerable<T9> Result9) QueryMultiple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IDbConnection cnn,
            IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();
        IEnumerable<T6> result6 = reader.Read<T6>();
        IEnumerable<T7> result7 = reader.Read<T7>();
        IEnumerable<T8> result8 = reader.Read<T8>();
        IEnumerable<T9> result9 = reader.Read<T9>();

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <typeparam name="T9">The type of the ninth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8,
            IEnumerable<T9> Result9)> QueryMultipleAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IDbConnection cnn,
            IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();
        IEnumerable<T6> result6 = await reader.ReadAsync<T6>();
        IEnumerable<T7> result7 = await reader.ReadAsync<T7>();
        IEnumerable<T8> result8 = await reader.ReadAsync<T8>();
        IEnumerable<T9> result9 = await reader.ReadAsync<T9>();

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <typeparam name="T9">The type of the ninth sql record set.</typeparam>
    /// <typeparam name="T10">The type of the tenth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static (IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
        IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8,
        IEnumerable<T9> Result9, IEnumerable<T10> Result10) QueryMultiple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbConnection cnn, IEnumerable<string> sqlList, object? param = null, CommandType? commandType = null)
    {
        using SqlMapper.GridReader reader = cnn.QueryMultiple(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = reader.Read<T1>();
        IEnumerable<T2> result2 = reader.Read<T2>();
        IEnumerable<T3> result3 = reader.Read<T3>();
        IEnumerable<T4> result4 = reader.Read<T4>();
        IEnumerable<T5> result5 = reader.Read<T5>();
        IEnumerable<T6> result6 = reader.Read<T6>();
        IEnumerable<T7> result7 = reader.Read<T7>();
        IEnumerable<T8> result8 = reader.Read<T8>();
        IEnumerable<T9> result9 = reader.Read<T9>();
        IEnumerable<T10> result10 = reader.Read<T10>();

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9, result10);
    }

    /// <summary>
    ///     Execute a command that returns multiple result sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">database connection</param>
    /// <param name="sqlList">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <typeparam name="T1">The type of the first sql record set.</typeparam>
    /// <typeparam name="T2">The type of the second sql record set.</typeparam>
    /// <typeparam name="T3">The type of the third sql record set.</typeparam>
    /// <typeparam name="T4">The type of the fourth sql record set.</typeparam>
    /// <typeparam name="T5">The type of the fifth sql record set.</typeparam>
    /// <typeparam name="T6">The type of the sixth sql record set.</typeparam>
    /// <typeparam name="T7">The type of the seventh sql record set.</typeparam>
    /// <typeparam name="T8">The type of the eighth sql record set.</typeparam>
    /// <typeparam name="T9">The type of the ninth sql record set.</typeparam>
    /// <typeparam name="T10">The type of the tenth sql record set.</typeparam>
    /// <returns>Multiple result sets</returns>
    public static async
        Task<(IEnumerable<T1> Result1, IEnumerable<T2> Result2, IEnumerable<T3> Result3, IEnumerable<T4> Result4,
            IEnumerable<T5> Result5, IEnumerable<T6> Result6, IEnumerable<T7> Result7, IEnumerable<T8> Result8,
            IEnumerable<T9> Result9, IEnumerable<T10> Result10)>
        QueryMultipleAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IDbConnection cnn, IEnumerable<string> sqlList,
            object? param = null, CommandType? commandType = null)
    {
        await using SqlMapper.GridReader
            reader = await cnn.QueryMultipleAsync(sqlList.Concat(), param, commandType: commandType);
        IEnumerable<T1> result1 = await reader.ReadAsync<T1>();
        IEnumerable<T2> result2 = await reader.ReadAsync<T2>();
        IEnumerable<T3> result3 = await reader.ReadAsync<T3>();
        IEnumerable<T4> result4 = await reader.ReadAsync<T4>();
        IEnumerable<T5> result5 = await reader.ReadAsync<T5>();
        IEnumerable<T6> result6 = await reader.ReadAsync<T6>();
        IEnumerable<T7> result7 = await reader.ReadAsync<T7>();
        IEnumerable<T8> result8 = await reader.ReadAsync<T8>();
        IEnumerable<T9> result9 = await reader.ReadAsync<T9>();
        IEnumerable<T10> result10 = await reader.ReadAsync<T10>();

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9, result10);
    }

    public static string Concat(this IEnumerable<string> sqlList)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var sql in sqlList)
        {
            _ = sql.EndsWith(';') ? sqlBuilder.Append(sql) : sqlBuilder.Append(sql).Append(';');
        }

        return sqlBuilder.ToString();
    }
}
#endif