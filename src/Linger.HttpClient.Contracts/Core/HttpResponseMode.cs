namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP 响应处理模式
/// </summary>
public enum HttpResponseMode
{
    /// <summary>
    /// 缓冲模式：等待整个响应体下载到内存后返回（默认）
    /// 适用于小型响应（JSON API 等）
    /// </summary>
    Buffered,

    /// <summary>
    /// 流式模式：收到响应头后立即返回，响应体可流式读取
    /// 适用于大文件下载、流式数据、进度报告等场景
    /// </summary>
    Streamed
}
