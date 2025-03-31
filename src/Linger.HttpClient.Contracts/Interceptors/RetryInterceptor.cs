using Linger.HttpClient.Contracts.Core;

namespace Linger.HttpClient.Contracts.Interceptors;

/// <summary>
/// 自动重试拦截器
/// </summary>
public class RetryInterceptor : IHttpClientInterceptor
{
    private readonly int _maxRetries;
    private readonly Func<HttpResponseMessage, bool> _shouldRetry;
    private readonly Func<int, Task> _delayFunc;

    /// <summary>
    /// 创建重试拦截器
    /// </summary>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="shouldRetry">判断是否应该重试的函数</param>
    /// <param name="delayFunc">延迟函数，参数为当前重试次数</param>
    public RetryInterceptor(
        int maxRetries = 3,
        Func<HttpResponseMessage, bool>? shouldRetry = null,
        Func<int, Task>? delayFunc = null)
    {
        _maxRetries = maxRetries;
        _shouldRetry = shouldRetry ?? DefaultShouldRetry;
        _delayFunc = delayFunc ?? DefaultDelayFunc;
    }

    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        // 在请求阶段不执行任何操作
        return Task.FromResult(request);
    }

    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        if (!_shouldRetry(response))
        {
            return response;
        }

        // 将响应信息保存起来，在重试失败时恢复
        var statusCode = response.StatusCode;
        var reasonPhrase = response.ReasonPhrase;
        var content = await response.Content.ReadAsStringAsync();

        // 重试逻辑
        var originalRequest = response.RequestMessage;
        if (originalRequest == null)
        {
            return response;
        }

        for (int retry = 0; retry < _maxRetries; retry++)
        {
            // 等待退避时间
            await _delayFunc(retry);

            // 创建新请求
            var newRequest = await CloneHttpRequestMessageAsync(originalRequest);

            // 重新发送请求
            var result = await newRequest.SendAsync(response.RequestMessage!.Content);

            // 如果成功或不需要再重试，返回结果
            if (!_shouldRetry(result))
            {
                return result;
            }
        }

        // 所有重试都失败，返回原始响应
        return response;
    }

    private static bool DefaultShouldRetry(HttpResponseMessage response)
    {
        return response.StatusCode == HttpStatusCode.ServiceUnavailable ||
               response.StatusCode == HttpStatusCode.GatewayTimeout ||
               response.StatusCode == (HttpStatusCode)429; // 429 is Too Many Requests
    }

    private static async Task DefaultDelayFunc(int retryAttempt)
    {
        // 指数退避策略，带随机抖动
        var delay = Math.Pow(2, retryAttempt) * 100;
        var jitter = new Random().Next(0, 100);
        await Task.Delay((int)delay + jitter);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // 复制头部
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 复制版本和属性
        clone.Version = request.Version;
        // 注意: 不再使用已弃用的Properties属性，改用Options
        // 由于Options API无法直接枚举所有选项，这里不再复制选项

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

// 扩展HttpRequestMessage以便发送请求
internal static class HttpRequestMessageExtensions
{
    public static async Task<HttpResponseMessage> SendAsync(this HttpRequestMessage request, HttpContent? content = null)
    {
        using var client = new System.Net.Http.HttpClient();

        if (content != null && request.Method != HttpMethod.Get)
        {
            request.Content = content;
        }

        return await client.SendAsync(request);
    }
}
