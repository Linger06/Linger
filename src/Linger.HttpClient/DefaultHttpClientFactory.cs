using System.Collections.Concurrent;
using Linger.HttpClient.Contracts;
using Linger.HttpClient.Contracts.Helpers;

namespace Linger.HttpClient;

/// <summary>
/// 默认HTTP客户端工厂实现
/// </summary>
public class DefaultHttpClientFactory : Contracts.IHttpClientFactory
{
    private readonly ConcurrentDictionary<string, Lazy<IHttpClient>> _clients = new();
    private readonly ConcurrentDictionary<string, (string BaseUrl, Action<HttpClientOptions>? ConfigureOptions)> _registrations = new();
    
    /// <summary>
    /// 创建一个新的HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <returns>HTTP客户端</returns>
    public IHttpClient CreateClient(string baseUrl)
    {
        return CreateClient(baseUrl, null);
    }
    
    /// <summary>
    /// 创建一个新的HTTP客户端并应用选项
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    /// <returns>HTTP客户端</returns>
    public IHttpClient CreateClient(string baseUrl, Action<HttpClientOptions>? configureOptions)
    {
        var options = new HttpClientOptions();
        configureOptions?.Invoke(options);
        
        var handler = CompressionHelper.CreateCompressionHandler();
        var httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        
        var client = new BaseHttpClient(httpClient);
        
        // 复制选项
        foreach (var header in options.DefaultHeaders)
        {
            client.AddHeader(header.Key, header.Value);
        }
        
        client.Options.DefaultTimeout = options.DefaultTimeout;
        client.Options.EnableRetry = options.EnableRetry;
        client.Options.MaxRetryCount = options.MaxRetryCount;
        client.Options.RetryInterval = options.RetryInterval;
        
        return client;
    }
    
    /// <summary>
    /// 获取或创建一个命名的HTTP客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>HTTP客户端</returns>
    public IHttpClient GetOrCreateClient(string name)
    {
        return _clients.GetOrAdd(name, clientName =>
        {
            if (!_registrations.TryGetValue(clientName, out var registration))
            {
                throw new InvalidOperationException($"未找到名为 '{clientName}' 的HTTP客户端注册");
            }
            
            return new Lazy<IHttpClient>(() => CreateClient(registration.BaseUrl, registration.ConfigureOptions));
        }).Value;
    }
    
    /// <summary>
    /// 注册一个命名的HTTP客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    public void RegisterClient(string name, string baseUrl, Action<HttpClientOptions>? configureOptions = null)
    {
        _registrations[name] = (baseUrl, configureOptions);
    }
}
