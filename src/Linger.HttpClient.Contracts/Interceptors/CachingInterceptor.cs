using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Core;

namespace Linger.HttpClient.Contracts.Interceptors;

/// <summary>
/// 缓存拦截器，缓存GET请求的响应
/// </summary>
public class CachingInterceptor : IHttpClientInterceptor
{
    private readonly ConcurrentDictionary<string, CachedResponse> _cache = new();
    private readonly TimeSpan _defaultCacheDuration;
    private readonly long _maxCacheSize;
    private readonly HashSet<string> _cacheableContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json", "text/plain", "text/html", "application/xml", "text/xml"
    };

    private readonly HashSet<string> _sensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "Set-Cookie"
    };

    /// <summary>
    /// 创建缓存拦截器
    /// </summary>
    /// <param name="defaultCacheDuration">默认缓存时间</param>
    /// <param name="maxCacheSize">最大缓存条目数，默认1000</param>
    public CachingInterceptor(TimeSpan? defaultCacheDuration = null, long maxCacheSize = 1000)
    {
        _defaultCacheDuration = defaultCacheDuration ?? TimeSpan.FromMinutes(5);
        _maxCacheSize = maxCacheSize;
    }

    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        // 只缓存GET请求
        if (request.Method != HttpMethod.Get)
        {
            return Task.FromResult(request);
        }

        // 如果请求指定了不缓存，则不使用缓存
        if (HasNoCacheHeader(request))
        {
            return Task.FromResult(request);
        }

        // 计算缓存键
        var cacheKey = CalculateCacheKey(request);

        // 检查是否有缓存
        if (_cache.TryGetValue(cacheKey, out var cachedResponse) && !cachedResponse.IsExpired())
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            // 设置这是一个缓存命中，但不需要发送请求
            request.Properties["SkipRequest"] = true;
            request.Properties["CachedResponse"] = cachedResponse.Response;
#else
            // 设置这是一个缓存命中，但不需要发送请求
            request.Options.Set(new HttpRequestOptionsKey<bool>("SkipRequest"), true);
            request.Options.Set(new HttpRequestOptionsKey<HttpResponseMessage>("CachedResponse"), cachedResponse.Response);
#endif
        }
#if NETFRAMEWORK || NETSTANDARD2_0
        // 在请求中保存缓存键，便于响应时使用
        request.Properties["CacheKey"] = cacheKey;
#else
        // 在请求中保存缓存键，便于响应时使用
        request.Options.Set(new HttpRequestOptionsKey<string>("CacheKey"), cacheKey);
#endif
        return Task.FromResult(request);
    }

    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        var request = response.RequestMessage;
        if (request == null)
        {
            return response;
        }

#if NETFRAMEWORK || NETSTANDARD2_0
        // 检查是否应该跳过请求并直接返回缓存
        if (request.Properties.TryGetValue("SkipRequest", out var skipObj) &&
            skipObj is bool skip && skip &&
            request.Properties.TryGetValue("CachedResponse", out var cachedResponseObj) &&
            cachedResponseObj is HttpResponseMessage cachedResponse)
#else
        // 检查是否应该跳过请求并直接返回缓存
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<bool>("SkipRequest"), out var skip) &&
            skip &&
            request.Options.TryGetValue(new HttpRequestOptionsKey<HttpResponseMessage>("CachedResponse"), out var cachedResponse))
#endif

        {
            // 返回缓存的响应的克隆，避免修改缓存的原始响应
            return await CloneHttpResponseMessageAsync(cachedResponse);
        }

        // 如果是成功的响应，并且是GET请求，考虑缓存
        if (response.IsSuccessStatusCode && request.Method == HttpMethod.Get)
        {
            // 不缓存不应该缓存的响应
            if (!ShouldCacheResponse(response))
            {
                return response;
            }
#if NETFRAMEWORK || NETSTANDARD2_0
            if (request.Properties.TryGetValue("CacheKey", out var cacheKeyObj) && cacheKeyObj is string cacheKey)
#else
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<string>("CacheKey"), out var cacheKey))
#endif
            {
                // 确保缓存不超过最大大小
                EnsureCacheSize();

                // 复制响应以便缓存
                var responseToCache = await CloneHttpResponseMessageAsync(response);

                // 确定缓存时间
                var cacheDuration = GetCacheDuration(response) ?? _defaultCacheDuration;

                // 保存到缓存
                _cache[cacheKey] = new CachedResponse(responseToCache, cacheDuration);
            }
        }

        return response;
    }

    /// <summary>
    /// 计算缓存键，考虑URL及重要的查询参数和请求头
    /// </summary>
    private string CalculateCacheKey(HttpRequestMessage request)
    {
        var uri = request.RequestUri?.ToString() ?? string.Empty;
        var headerValues = new StringBuilder();

        // 添加可能影响响应内容的请求头
        foreach (var header in request.Headers)
        {
            // 跳过敏感或认证相关的头部
            if (_sensitiveHeaders.Contains(header.Key))
            {
                continue;
            }

            // 只考虑可能影响内容的头部
            if (header.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase))
            {
                headerValues.Append($"{header.Key}:{string.Join(",", header.Value)}|");
            }
        }

        var keyInput = $"{uri}|{headerValues}";
        var hash = keyInput.ToMd5HashByte();
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 确定是否应该缓存响应
    /// </summary>
    private bool ShouldCacheResponse(HttpResponseMessage response)
    {
        // 检查Cache-Control头  
        if (response.Headers.CacheControl != null)
        {
            // 不缓存明确标记为no-store的响应  
            if (response.Headers.CacheControl.NoStore)
            {
                return false;
            }

            // 包含private指令的响应通常包含个人信息  
            if (response.Headers.CacheControl.Private)
            {
                return false;
            }
        }

        // 检查内容类型  
        if (response.Content?.Headers?.ContentType != null)
        {
            var contentType = response.Content.Headers.ContentType.MediaType;
            if (contentType != null && !_cacheableContentTypes.Any(ct => contentType.StartsWith(ct, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检查请求是否指定了不使用缓存
    /// </summary>
    private static bool HasNoCacheHeader(HttpRequestMessage request)
    {
        if (request.Headers.CacheControl != null)
        {
            return request.Headers.CacheControl.NoCache;
        }

        // 检查Pragma: no-cache (HTTP/1.0兼容)
        return request.Headers.Pragma.Any(p => p.Name.Equals("no-cache", StringComparison.OrdinalIgnoreCase));
    }

    private static TimeSpan? GetCacheDuration(HttpResponseMessage response)
    {
        // 从Cache-Control头部获取max-age值
        if (response.Headers.CacheControl?.MaxAge != null)
        {
            return response.Headers.CacheControl.MaxAge;
        }

        // 从Expires头部计算缓存时间
        if (response.Headers.CacheControl?.Public == true &&
            response.Content.Headers.Expires != null)
        {
            var expiresDate = response.Content.Headers.Expires.Value;
            var now = DateTimeOffset.UtcNow;
            if (expiresDate > now)
            {
                return expiresDate - now;
            }
        }

        return null;
    }

    private static async Task<HttpResponseMessage> CloneHttpResponseMessageAsync(HttpResponseMessage response)
    {
        var clone = new HttpResponseMessage(response.StatusCode)
        {
            ReasonPhrase = response.ReasonPhrase,
            Version = response.Version
        };

        // 复制头部
        foreach (var header in response.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 复制内容
        if (response.Content != null)
        {
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var clonedContent = new ByteArrayContent(contentBytes);

            // 复制内容头
            foreach (var header in response.Content.Headers)
            {
                clonedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = clonedContent;
        }

        return clone;
    }

    /// <summary>
    /// 确保缓存不超过最大大小
    /// </summary>
    private void EnsureCacheSize()
    {
        if (_cache.Count <= _maxCacheSize)
        {
            return;
        }

        // 如果超出大小，移除约20%的最早过期的条目
        var itemsToRemove = (int)(_maxCacheSize * 0.2);
        var sortedItems = _cache.OrderBy(kvp => kvp.Value.ExpiryTime).Take(itemsToRemove).ToList();

        foreach (var item in sortedItems)
        {
            _cache.TryRemove(item.Key, out _);
        }
    }

    private class CachedResponse
    {
        public HttpResponseMessage Response { get; }
        public DateTime ExpiryTime { get; }

        public CachedResponse(HttpResponseMessage response, TimeSpan duration)
        {
            Response = response;
            ExpiryTime = DateTime.UtcNow.Add(duration);
        }

        public bool IsExpired() => DateTime.UtcNow > ExpiryTime;
    }
}
