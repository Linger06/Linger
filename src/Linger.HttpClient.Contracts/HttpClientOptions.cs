using System.Collections.Generic;

namespace Linger.HttpClient.Contracts;

/// <summary>
/// HTTP客户端配置选项
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// 默认超时时间（秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 30;
    
    /// <summary>
    /// 是否启用重试
    /// </summary>
    public bool EnableRetry { get; set; } = false;
    
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// 重试间隔（毫秒）
    /// </summary>
    public int RetryInterval { get; set; } = 1000;
    
    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
}
