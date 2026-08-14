using System.Data.Common;

namespace Linger.DataAccess;

/// <summary>
/// 收敛仅在现代 .NET 上可用的 ADO.NET 异步 API。
/// </summary>
/// <remarks>
/// .NET Framework 4.7.2 不编译事务异步成员，避免将同步事务伪装为异步操作。
/// </remarks>
#if !NET472
internal static class DbCompat
{
    internal static async Task<DbTransaction> BeginTransactionAsync(DbConnection connection,
        CancellationToken cancellationToken)
    {
        return await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static async Task CommitAsync(DbTransaction transaction, CancellationToken cancellationToken)
    {
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static async Task RollbackAsync(DbTransaction transaction, CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }

}
#endif
