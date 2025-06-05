using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// JWT令牌黑名单，用于存储已撤销的令牌
/// </summary>
public class JwtTokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();
    private readonly ILogger<JwtTokenBlacklist>? _logger;

    public JwtTokenBlacklist(ILogger<JwtTokenBlacklist>? logger = null)
    {
        _logger = logger;
        // 每小时清理一次过期的黑名单条目
        new Timer(CleanupExpiredEntries, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    /// <summary>
    /// 将令牌添加到黑名单
    /// </summary>
    /// <param name="tokenId">令牌ID(jti声明)</param>
    /// <param name="expiryTime">令牌过期时间</param>
    public void Add(string tokenId, DateTime expiryTime)
    {
        if (_blacklist.TryAdd(tokenId, expiryTime))
        {
            _logger?.LogInformation("令牌已加入黑名单: {TokenId}, 过期时间: {ExpiryTime}", tokenId, expiryTime);
        }
    }

    /// <summary>
    /// 检查令牌是否在黑名单中
    /// </summary>
    /// <param name="tokenId">令牌ID</param>
    /// <returns>如果在黑名单中返回true</returns>
    public bool Contains(string tokenId)
    {
        return _blacklist.ContainsKey(tokenId);
    }

    /// <summary>
    /// 从黑名单中移除令牌（通常在令牌过期后自动清理）
    /// </summary>
    /// <param name="tokenId">令牌ID</param>
    public void Remove(string tokenId)
    {
        if (_blacklist.TryRemove(tokenId, out _))
        {
            _logger?.LogDebug("令牌已从黑名单移除: {TokenId}", tokenId);
        }
    }

    private void CleanupExpiredEntries(object? state)
    {
        int removedCount = 0;
        foreach (var entry in _blacklist)
        {
            if (entry.Value <= DateTime.UtcNow)
            {
                if (_blacklist.TryRemove(entry.Key, out _))
                {
                    removedCount++;
                }
            }
        }

        if (removedCount > 0)
        {
            _logger?.LogInformation("已清理 {Count} 个过期的黑名单条目", removedCount);
        }
    }
}
