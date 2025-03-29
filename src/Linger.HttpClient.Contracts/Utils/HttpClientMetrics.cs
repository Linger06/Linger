using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Linger.HttpClient.Contracts.Utils;

/// <summary>
/// HTTP客户端性能指标监控
/// </summary>
public class HttpClientMetrics
{
    private static readonly Lazy<HttpClientMetrics> _instance = new(() => new HttpClientMetrics());
    
    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static HttpClientMetrics Instance => _instance.Value;
    
    private readonly ConcurrentDictionary<string, EndpointMetrics> _metrics = new();
    
    /// <summary>
    /// 记录请求开始
    /// </summary>
    /// <param name="endpoint">端点</param>
    /// <returns>跟踪标识符</returns>
    public Guid StartRequest(string endpoint)
    {
        var metrics = _metrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
        return metrics.StartRequest();
    }
    
    /// <summary>
    /// 记录请求完成
    /// </summary>
    /// <param name="endpoint">端点</param>
    /// <param name="requestId">请求ID</param>
    /// <param name="success">是否成功</param>
    public void EndRequest(string endpoint, Guid requestId, bool success)
    {
        if (_metrics.TryGetValue(endpoint, out var metrics))
        {
            metrics.EndRequest(requestId, success);
        }
    }
    
    /// <summary>
    /// 获取所有端点的性能指标
    /// </summary>
    public Dictionary<string, EndpointStats> GetAllStats()
    {
        var result = new Dictionary<string, EndpointStats>();
        
        foreach (var entry in _metrics)
        {
            result[entry.Key] = entry.Value.GetStats();
        }
        
        return result;
    }
    
    /// <summary>
    /// 获取特定端点的性能指标
    /// </summary>
    /// <param name="endpoint">端点</param>
    public EndpointStats? GetEndpointStats(string endpoint)
    {
        return _metrics.TryGetValue(endpoint, out var metrics) ? metrics.GetStats() : null;
    }
    
    /// <summary>
    /// 重置所有指标
    /// </summary>
    public void Reset()
    {
        _metrics.Clear();
    }
    
    /// <summary>
    /// 端点性能指标
    /// </summary>
    private class EndpointMetrics
    {
        private readonly ConcurrentDictionary<Guid, Stopwatch> _activeRequests = new();
        private long _totalRequests;
        private long _successfulRequests;
        private long _failedRequests;
        private long _totalResponseTime;
        private long _minResponseTime = long.MaxValue;
        private long _maxResponseTime;
        
        /// <summary>
        /// 开始请求
        /// </summary>
        public Guid StartRequest()
        {
            var requestId = Guid.NewGuid();
            _activeRequests[requestId] = Stopwatch.StartNew();
            Interlocked.Increment(ref _totalRequests);
            return requestId;
        }
        
        /// <summary>
        /// 结束请求
        /// </summary>
        public void EndRequest(Guid requestId, bool success)
        {
            if (_activeRequests.TryRemove(requestId, out var stopwatch))
            {
                stopwatch.Stop();
                
                // 更新计数器
                if (success)
                    Interlocked.Increment(ref _successfulRequests);
                else
                    Interlocked.Increment(ref _failedRequests);
                
                // 更新响应时间统计
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                Interlocked.Add(ref _totalResponseTime, elapsedMs);
                
                // 更新最小响应时间
                InterlockedMin(ref _minResponseTime, elapsedMs);
                
                // 更新最大响应时间
                InterlockedMax(ref _maxResponseTime, elapsedMs);
            }
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public EndpointStats GetStats()
        {
            var total = Interlocked.Read(ref _totalRequests);
            var successful = Interlocked.Read(ref _successfulRequests);
            var failed = Interlocked.Read(ref _failedRequests);
            var totalTime = Interlocked.Read(ref _totalResponseTime);
            var minTime = Interlocked.Read(ref _minResponseTime);
            var maxTime = Interlocked.Read(ref _maxResponseTime);
            
            // 计算成功率
            var successRate = total > 0 ? (double)successful / total : 0;
            
            // 计算平均响应时间
            var avgTime = total > 0 ? (double)totalTime / total : 0;
            
            // 如果没有请求，设置最小响应时间为0
            if (minTime == long.MaxValue)
                minTime = 0;
            
            return new EndpointStats
            {
                TotalRequests = total,
                SuccessfulRequests = successful,
                FailedRequests = failed,
                SuccessRate = successRate,
                AverageResponseTime = avgTime,
                MinResponseTime = minTime,
                MaxResponseTime = maxTime,
                ActiveRequests = _activeRequests.Count
            };
        }
        
        /// <summary>
        /// 线程安全的最小值计算
        /// </summary>
        private static void InterlockedMin(ref long target, long value)
        {
            var current = Interlocked.Read(ref target);
            while (value < current)
            {
                var exchanged = Interlocked.CompareExchange(ref target, value, current);
                if (exchanged == current)
                    break;
                current = exchanged;
            }
        }
        
        /// <summary>
        /// 线程安全的最大值计算
        /// </summary>
        private static void InterlockedMax(ref long target, long value)
        {
            var current = Interlocked.Read(ref target);
            while (value > current)
            {
                var exchanged = Interlocked.CompareExchange(ref target, value, current);
                if (exchanged == current)
                    break;
                current = exchanged;
            }
        }
    }
}

/// <summary>
/// 端点统计数据
/// </summary>
public class EndpointStats
{
    /// <summary>
    /// 总请求数
    /// </summary>
    public long TotalRequests { get; set; }
    
    /// <summary>
    /// 成功的请求数
    /// </summary>
    public long SuccessfulRequests { get; set; }
    
    /// <summary>
    /// 失败的请求数
    /// </summary>
    public long FailedRequests { get; set; }
    
    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }
    
    /// <summary>
    /// 平均响应时间(毫秒)
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// 最小响应时间(毫秒)
    /// </summary>
    public long MinResponseTime { get; set; }
    
    /// <summary>
    /// 最大响应时间(毫秒)
    /// </summary>
    public long MaxResponseTime { get; set; }
    
    /// <summary>
    /// 当前活跃请求数
    /// </summary>
    public int ActiveRequests { get; set; }
}
