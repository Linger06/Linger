using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Linger.Exceptions;
using Linger.Helper;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Interceptors;

/// <summary>
/// 自动重试拦截器
/// </summary>
public class RetryInterceptor : IHttpClientInterceptor
{
    private readonly HttpClientOptions _options;
    private readonly Func<HttpResponseMessage, bool> _shouldRetry;
    private readonly RetryHelper _retryHelper;
    
    /// <summary>
    /// 创建重试拦截器
    /// </summary>
    /// <param name="options">HTTP客户端选项</param>
    /// <param name="shouldRetry">判断是否应该重试的函数</param>
    public RetryInterceptor(
        HttpClientOptions options,
        Func<HttpResponseMessage, bool>? shouldRetry = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _shouldRetry = shouldRetry ?? DefaultShouldRetry;
        
        // 创建RetryOptions并配置
        var retryOptions = new RetryOptions
        {
            MaxRetries = _options.MaxRetryCount,
            BaseDelayMs = _options.RetryInterval,
            UseExponentialBackoff = true, // 使用指数退避策略
            JitterFactor = 0.2 // 添加20%的随机抖动
        };
        
        _retryHelper = new RetryHelper(retryOptions);
    }
    
    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        // 在请求阶段不执行任何操作
        return Task.FromResult(request);
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        // 如果重试被禁用或不满足重试条件，则直接返回
        if (!_options.EnableRetry || !_shouldRetry(response))
        {
            return response;
        }
        
        var originalRequest = response.RequestMessage;
        if (originalRequest == null)
        {
            return response;
        }
        
        // 使用RetryHelper进行重试
        try
        {
            return await _retryHelper.ExecuteAsync(
                async () => {
                    // 创建新请求
                    var newRequest = await CloneHttpRequestMessageAsync(originalRequest);
                    
                    // 使用新的HttpClient发送请求
                    using var client = new System.Net.Http.HttpClient();
                    return await client.SendAsync(newRequest);
                },
                "HTTP Response Retry",
                ex => ex is HttpRequestException || ex is TaskCanceledException,
                default);
        }
        catch (OutOfReTryCountException)
        {
            // 所有重试都失败，返回原始响应
            return response;
        }
    }
    
    private static bool DefaultShouldRetry(HttpResponseMessage response)
    {
        return response.StatusCode == HttpStatusCode.ServiceUnavailable ||
               response.StatusCode == HttpStatusCode.GatewayTimeout ||
               response.StatusCode == (HttpStatusCode)429; // 429 is Too Many Requests
    }
    
    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        
        // 复制头部
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        // 复制版本
        clone.Version = request.Version;
        
        // 复制内容
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            var clonedContent = new ByteArrayContent(contentBytes);
            
            // 复制内容头
            foreach (var header in request.Content.Headers)
            {
                clonedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            clone.Content = clonedContent;
        }
        
        return clone;
    }
}
