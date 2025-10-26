using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Linger.EFCore.Audit.Interceptors;

/// <summary>
/// Intercepts and logs SQL commands for profiling and debugging purposes
/// </summary>
public class SqlCommandProfilerInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SqlCommandProfilerInterceptor>? _logger;
    private readonly bool _includeParameters;

    /// <summary>
    /// Initializes a new instance with parameter logging enabled
    /// </summary>
    public SqlCommandProfilerInterceptor(ILogger<SqlCommandProfilerInterceptor>? logger = null)
        : this(true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance with configurable parameter logging
    /// </summary>
    /// <param name="includeParameters">Whether to include parameter values in logs</param>
    /// <param name="logger">Optional logger instance</param>
    public SqlCommandProfilerInterceptor(bool includeParameters, ILogger<SqlCommandProfilerInterceptor>? logger = null)
    {
        _includeParameters = includeParameters;
        _logger = logger;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        LogCommand(command, "ReaderExecuting");
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        LogCommand(command, "ReaderExecutingAsync");
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result)
    {
        LogCommand(command, "NonQueryExecuting");
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        LogCommand(command, "NonQueryExecutingAsync");
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result)
    {
        LogCommand(command, "ScalarExecuting");
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        LogCommand(command, "ScalarExecutingAsync");
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void LogCommand(DbCommand command, string operation)
    {
        if (_logger == null || !_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        if (_includeParameters && command.Parameters.Count > 0)
        {
            var fullCommandText = command.CommandText;

            foreach (DbParameter param in command.Parameters)
            {
                var paramValue = param.Value switch
                {
                    string => $"'{param.Value}'",
                    null => "NULL",
                    _ => param.Value.ToString()
                };
                fullCommandText = fullCommandText.Replace(param.ParameterName, paramValue);
            }

            _logger.LogDebug("[{Operation}] SQL with parameters: {CommandText}", operation, fullCommandText);
        }
        else
        {
            _logger.LogDebug("[{Operation}] SQL: {CommandText}", operation, command.CommandText);
        }
    }
}