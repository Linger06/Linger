using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Linger.EFCore.Audit.Interceptors;

public class SqlCommandProfilerInterceptor : DbCommandInterceptor
{
    // Before
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        var fullCommandText = command.CommandText;

        foreach (DbParameter param in command.Parameters)
        {
            var paramValue = param.Value switch
            {
                string => $"'{param.Value}'",
                null => "Null",
                _ => param.Value.ToString()
            };
            fullCommandText = fullCommandText.Replace(param.ParameterName, paramValue);
        }

        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine($"Executing query: {fullCommandText}");
        Debug.WriteLine("++++++++++++++++++++");
        return base.ReaderExecuting(command, eventData, result);
    }

    // Before 
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var sql = command.CommandText;
        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine("ReaderExecutingAsync:\r\n" + sql);
        Debug.WriteLine("++++++++++++++++++++");
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result)
    {
        var sql = command.CommandText;
        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine("NonQueryExecuting:\r\n" + sql);
        Debug.WriteLine("++++++++++++++++++++");
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {

        var fullCommandText = command.CommandText;

        foreach (DbParameter param in command.Parameters)
        {
            var paramValue = param.Value switch
            {
                string => $"'{param.Value}'",
                null => "Null",
                _ => param.Value.ToString()
            };
            fullCommandText = fullCommandText.Replace(param.ParameterName, paramValue);
        }

        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine($"Executing NonQuery: {fullCommandText}");
        Debug.WriteLine("++++++++++++++++++++");
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result)
    {
        var sql = command.CommandText;
        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine("ScalarExecuting:\r\n" + sql);
        Debug.WriteLine("++++++++++++++++++++");
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        var sql = command.CommandText;
        Debug.WriteLine("++++++++++++++++++++");
        Debug.WriteLine("ScalarExecutingAsync:\r\n" + sql);
        Debug.WriteLine("++++++++++++++++++++");
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}