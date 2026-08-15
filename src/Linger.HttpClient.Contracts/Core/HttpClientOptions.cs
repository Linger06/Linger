namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端配置选项
/// </summary>
[Obsolete("HttpClientOptions will be removed in 2.0.0. Configure the underlying HttpClient directly instead. See the migration guide in the Linger repository for guidance.")]
public class HttpClientOptions
{
    /// <summary>
    /// 默认超时时间(秒)
    /// </summary>
    public int DefaultTimeout { get; set; } = 30;

    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = [];
}
