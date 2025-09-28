namespace Linger.HttpClient.Contracts.Models;

/// <summary>
/// 表示包含简单错误键值集合的 ProblemDetails 响应模型。
/// </summary>
/// <remarks>
/// 该类型用于 HTTP API 错误返回的标准化包装，<see cref="Errors"/> 字典可用于承载字段级错误信息。
/// </remarks>
/// <example>
/// <code>
/// var problem = new ProblemDetailsWithErrors
/// {
///     Type = "https://httpstatuses.com/400",
///     Title = "Validation Failed",
///     Status = 400,
///     Errors = { ["UserName"] = "Required" }
/// };
/// </code>
/// </example>
public class ProblemDetailsWithErrors
{
    /// <summary>
    /// 错误类型的 URI（可用于文档定位）。
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 人类可读的错误标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 状态码。
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 字段或成员级的错误集合 (key 为字段名, value 为错误描述)。
    /// </summary>
    public Dictionary<string, string> Errors { get; set; } = [];
}
