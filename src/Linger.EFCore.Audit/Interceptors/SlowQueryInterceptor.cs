using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Linger.EFCore.Audit.Interceptors;

/// <summary>
/// Intercepts database commands to identify and log slow queries
/// </summary>
public class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor>? _logger;
    private readonly int _slowQueryThresholdMs;

    /// <summary>
    /// Initializes a new instance with default threshold of 200ms
    /// </summary>
    public SlowQueryInterceptor(ILogger<SlowQueryInterceptor>? logger = null)
        : this(200, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance with custom threshold
    /// </summary>
    /// <param name="slowQueryThresholdMs">Threshold in milliseconds to identify slow queries</param>
    /// <param name="logger">Optional logger instance</param>
    public SlowQueryInterceptor(int slowQueryThresholdMs, ILogger<SlowQueryInterceptor>? logger = null)
    {
        _slowQueryThresholdMs = slowQueryThresholdMs;
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (eventData.Duration.TotalMilliseconds > _slowQueryThresholdMs)
        {
            _logger?.LogWarning("Slow query detected: {Duration}ms (threshold: {Threshold}ms) - {CommandText}",
                eventData.Duration.TotalMilliseconds,
                _slowQueryThresholdMs,
                command.CommandText);
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken cancellationToken = default)
    {
        if (eventData.Duration.TotalMilliseconds > _slowQueryThresholdMs)
        {
            _logger?.LogWarning("Slow query detected: {Duration}ms (threshold: {Threshold}ms) - {CommandText}",
                eventData.Duration.TotalMilliseconds,
                _slowQueryThresholdMs,
                command.CommandText);
        }

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken).ConfigureAwait(false);
    }
}