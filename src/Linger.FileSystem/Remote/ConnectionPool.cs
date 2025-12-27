using System.Collections.Concurrent;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 通用异步连接池，用于复用 FTP/SFTP 等客户端连接。
/// </summary>
/// <typeparam name="T">连接类型</typeparam>
/// <remarks>
/// <para>池大小由构造时的 poolSize 参数决定，通常与 MaxDegreeOfParallelism 一致。</para>
/// <para>调用方通过 <see cref="RentAsync"/> 获取连接，用完后通过 <see cref="Return"/> 归还。</para>
/// <para>当池中无可用连接时，会通过 factory 创建新连接（最多创建 poolSize 个）。</para>
/// <para>支持空闲超时：设置 <see cref="MaxIdleTime"/> 后，超时的连接会在租借时被丢弃。</para>
/// </remarks>
/// <example>
/// <code>
/// var pool = new ConnectionPool&lt;AsyncFtpClient&gt;(
///     poolSize: 4,
///     factory: async ct => { var c = new AsyncFtpClient(...); await c.AutoConnect(); return c; },
///     healthCheck: c => c.IsConnected,
///     dispose: async c => { await c.Disconnect(); c.Dispose(); },
///     maxIdleTime: TimeSpan.FromMinutes(5)
/// );
/// 
/// var client = await pool.RentAsync(cancellationToken);
/// try
/// {
///     // 使用 client 执行操作
/// }
/// finally
/// {
///     pool.Return(client);
/// }
/// 
/// await pool.DisposeAsync(); // 清理所有连接
/// </code>
/// </example>
public sealed class ConnectionPool<T> : IAsyncDisposable where T : class
{
    private readonly int _poolSize;
    private readonly Func<CancellationToken, Task<T>> _factory;
    private readonly Func<T, bool>? _healthCheck;
    private readonly Func<T, Task>? _disposeAsync;
    private readonly Action<T>? _disposeSync;
    private readonly TimeSpan? _maxIdleTime;

    private readonly ConcurrentBag<PooledConnection> _available = new();
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    /// <summary>
    /// 获取或设置最大空闲时间。超过此时间的连接在租借时会被丢弃并重新创建。
    /// </summary>
    public TimeSpan? MaxIdleTime => _maxIdleTime;

    /// <summary>
    /// 初始化连接池
    /// </summary>
    /// <param name="poolSize">池大小（最大连接数）</param>
    /// <param name="factory">创建新连接的工厂方法</param>
    /// <param name="healthCheck">连接健康检查（可选），返回 false 时连接会被丢弃并重新创建</param>
    /// <param name="disposeAsync">异步释放连接的方法（可选）</param>
    /// <param name="disposeSync">同步释放连接的方法（可选，当 disposeAsync 未提供时使用）</param>
    /// <param name="maxIdleTime">最大空闲时间（可选），超时的连接会被丢弃</param>
    public ConnectionPool(
        int poolSize,
        Func<CancellationToken, Task<T>> factory,
        Func<T, bool>? healthCheck = null,
        Func<T, Task>? disposeAsync = null,
        Action<T>? disposeSync = null,
        TimeSpan? maxIdleTime = null)
    {
        if (poolSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(poolSize), "Pool size must be greater than 0.");
        }

        if (maxIdleTime.HasValue && maxIdleTime.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIdleTime), "Max idle time must be greater than zero.");
        }

        _poolSize = poolSize;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _healthCheck = healthCheck;
        _disposeAsync = disposeAsync;
        _disposeSync = disposeSync;
        _maxIdleTime = maxIdleTime;
        _semaphore = new SemaphoreSlim(poolSize, poolSize);
    }

    /// <summary>
    /// 从池中租借一个连接。如果池中无可用连接且未达到上限，会创建新连接。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可用的连接实例</returns>
    public async Task<T> RentAsync(CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConnectionPool<T>));
        }
#endif

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        // 尝试从池中获取
        while (_available.TryTake(out var pooledConn))
        {
            var conn = pooledConn.Connection;

            // 空闲超时检查
            if (_maxIdleTime.HasValue && pooledConn.IsIdleTimedOut(_maxIdleTime.Value))
            {
                await DisposeConnectionAsync(conn).ConfigureAwait(false);
                continue;
            }

            // 健康检查
            if (_healthCheck is null || _healthCheck(conn))
            {
                return conn;
            }

            // 连接不健康，丢弃
            await DisposeConnectionAsync(conn).ConfigureAwait(false);
        }

        // 池中没有可用连接，创建新的
        var newConn = await _factory(cancellationToken).ConfigureAwait(false);

        return newConn;
    }

    /// <summary>
    /// 归还连接到池中
    /// </summary>
    /// <param name="connection">要归还的连接</param>
    public void Return(T connection)
    {
        if (connection is null)
        {
            return;
        }

        if (_disposed)
        {
            // 池已释放，直接丢弃连接
            _ = DisposeConnectionAsync(connection);

            return;
        }

        // 健康检查后决定是否放回池中
        if (_healthCheck is not null && !_healthCheck(connection))
        {
            _ = DisposeConnectionAsync(connection);
        }
        else
        {
            _available.Add(new PooledConnection(connection));
        }

        _semaphore.Release();
    }

    /// <summary>
    /// 释放池中所有连接
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // 释放所有可用连接
        while (_available.TryTake(out var pooledConn))
        {
            await DisposeConnectionAsync(pooledConn.Connection).ConfigureAwait(false);
        }

        _semaphore.Dispose();
    }

    private async Task DisposeConnectionAsync(T connection)
    {
        try
        {
            if (_disposeAsync is not null)
            {
                await _disposeAsync(connection).ConfigureAwait(false);
            }
            else if (_disposeSync is not null)
            {
                _disposeSync(connection);
            }
            else if (connection is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else if (connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        catch
        {
            // 忽略释放时的异常
        }
    }

    /// <summary>
    /// 包装连接及其返回时间，用于空闲超时检查
    /// </summary>
    private readonly struct PooledConnection
    {
        public T Connection { get; }
        public DateTime ReturnedAt { get; }

        public PooledConnection(T connection)
        {
            Connection = connection;
            ReturnedAt = DateTime.UtcNow;
        }

        public bool IsIdleTimedOut(TimeSpan maxIdleTime)
        {
            return DateTime.UtcNow - ReturnedAt > maxIdleTime;
        }
    }
}
