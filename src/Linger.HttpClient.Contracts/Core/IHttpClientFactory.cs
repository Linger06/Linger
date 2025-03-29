using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端工厂接口
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// 创建一个新的HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <returns>HTTP客户端</returns>
    IHttpClient CreateClient(string baseUrl);
    
    /// <summary>
    /// 创建一个新的HTTP客户端并应用选项
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    /// <returns>HTTP客户端</returns>
    IHttpClient CreateClient(string baseUrl, Action<HttpClientOptions> configureOptions);
    
    /// <summary>
    /// 获取或创建一个命名的HTTP客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>HTTP客户端</returns>
    IHttpClient GetOrCreateClient(string name);
    
    /// <summary>
    /// 注册一个命名的HTTP客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    void RegisterClient(string name, string baseUrl, Action<HttpClientOptions>? configureOptions = null);
}