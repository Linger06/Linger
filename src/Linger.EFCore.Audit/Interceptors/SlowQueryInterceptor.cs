using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Linger.EFCore.Audit.Interceptors;

public class SlowQueryInterceptor : DbCommandInterceptor
{
    private const int SlowQueryThreshold = 200;

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThreshold)
        {
            Console.WriteLine($"Slow query ({eventData.Duration.TotalMilliseconds} ms): {command.CommandText}");
        }

        return base.ReaderExecuted(command, eventData, result);
    }
}