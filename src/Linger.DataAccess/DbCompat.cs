using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
/// 把 net472 缺失的 ADO.NET 异步 API 收敛到一处。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DbConnection.BeginTransactionAsync(CancellationToken)"/>、
/// <see cref="DbTransaction.CommitAsync(CancellationToken)"/>、
/// <see cref="DbTransaction.RollbackAsync(CancellationToken)"/> 与
/// <see cref="DbConnection.DisposeAsync"/> 均为 .NET Core 3.0 起才有，net472 上只能退回同步实现。
/// </para>
/// <para>
/// 这些包装原先在 <see cref="BaseDatabase"/> 与 <see cref="Database"/> 中各存一份，
/// 条件编译分散在两处很容易只改一边。集中在此后，<c>#if NET472</c> 在本程序集中只出现在这个文件里。
/// </para>
/// </remarks>
internal static class DbCompat
{
    internal static async Task<DbTransaction> BeginTransactionAsync(DbConnection connection,
        CancellationToken cancellationToken)
    {
#if NET472
        await Task.CompletedTask.ConfigureAwait(false);
        return connection.BeginTransaction();
#else
        return await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    internal static async Task CommitAsync(DbTransaction transaction, CancellationToken cancellationToken)
    {
#if NET472
        await Task.CompletedTask.ConfigureAwait(false);
        transaction.Commit();
#else
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    internal static async Task RollbackAsync(DbTransaction transaction, CancellationToken cancellationToken)
    {
#if NET472
        await Task.CompletedTask.ConfigureAwait(false);
        transaction.Rollback();
#else
        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    internal static async Task DisposeAsync(DbConnection connection)
    {
#if NET472
        connection.Dispose();
        await Task.CompletedTask.ConfigureAwait(false);
#else
        await connection.DisposeAsync().ConfigureAwait(false);
#endif
    }
}
