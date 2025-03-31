namespace Linger.HttpClient.Contracts.Helpers;

/// <summary>
/// HTTP压缩辅助类
/// </summary>
public static class CompressionHelper
{
    /// <summary>
    /// 创建支持gzip和deflate压缩的请求消息处理器
    /// </summary>
    /// <param name="innerHandler">内部处理器</param>
    /// <returns>支持压缩的处理器</returns>
    public static HttpClientHandler CreateCompressionHandler(HttpMessageHandler? innerHandler = null)
    {
        var handler = innerHandler as HttpClientHandler ?? new HttpClientHandler();

        if (handler.SupportsAutomaticDecompression)
        {
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                                            System.Net.DecompressionMethods.Deflate;
        }

        return handler;
    }

    /// <summary>
    /// 为请求添加压缩支持
    /// </summary>
    /// <param name="request">HTTP请求</param>
    /// <returns>启用压缩的请求</returns>
    public static HttpRequestMessage EnableCompression(this HttpRequestMessage request)
    {
        if (request.Headers.AcceptEncoding.Count == 0)
        {
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        }

        return request;
    }
}
