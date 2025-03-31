using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Helpers;
using Linger.HttpClient.Contracts.Interceptors;
using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Factories
{
    /// <summary>
    /// 默认拦截器工厂，根据配置创建适当的拦截器
    /// </summary>
    public static class DefaultInterceptorFactory
    {
        /// <summary>
        /// 根据选项创建标准拦截器集
        /// </summary>
        /// <param name="options">HTTP客户端选项</param>
        /// <returns>创建的拦截器列表</returns>
        public static IEnumerable<IHttpClientInterceptor> CreateStandardInterceptors(HttpClientOptions options)
        {
            var interceptors = new List<IHttpClientInterceptor>();
            
            // 启用压缩
            interceptors.Add(new CompressionInterceptor());
            
            // 如果启用了重试，添加重试拦截器
            if (options.EnableRetry)
            {
                interceptors.Add(new RetryInterceptor(options));
            }
            
            return interceptors;
        }
    }
    
    /// <summary>
    /// 压缩拦截器 - 为所有请求添加压缩支持
    /// </summary>
    public class CompressionInterceptor : IHttpClientInterceptor
    {
        public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
        {
            // 使用CompressionHelper为请求添加压缩支持
            return Task.FromResult(CompressionHelper.EnableCompression(request));
        }

        public Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
        {
            // 响应阶段不做处理
            return Task.FromResult(response);
        }
    }
}
