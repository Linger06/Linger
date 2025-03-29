#if NETFRAMEWORK
using System.Net.Http;
#endif

using Linger.HttpClient.Contracts.Core;

/// <summary>
/// 用于记录HTTP请求和响应的拦截器
/// </summary>
public class LoggingInterceptor : IHttpClientInterceptor
{
    private readonly Action<string> _logger;

    public LoggingInterceptor(Action<string> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        _logger($"HTTP请求: {request.Method} {request.RequestUri}");
        return Task.FromResult(request);
    }

    public Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        _logger($"HTTP响应: {(int)response.StatusCode} {response.StatusCode} 来自 {response.RequestMessage?.RequestUri}");
        return Task.FromResult(response);
    }
}
