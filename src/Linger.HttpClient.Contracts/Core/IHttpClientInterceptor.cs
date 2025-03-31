namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端拦截器接口
/// </summary>
public interface IHttpClientInterceptor
{
    /// <summary>
    /// 请求前拦截
    /// </summary>
    /// <param name="request">HTTP请求消息</param>
    /// <returns>处理后的HTTP请求消息</returns>
    Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request);

    /// <summary>
    /// 响应后拦截
    /// </summary>
    /// <param name="response">HTTP响应消息</param>
    /// <returns>处理后的HTTP响应消息</returns>
    Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response);
}