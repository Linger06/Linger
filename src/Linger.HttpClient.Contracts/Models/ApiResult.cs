namespace Linger.HttpClient.Contracts.Models;

/// <summary>
/// API调用结果（无返回值场景）
/// </summary>
public class ApiResult
{
    public HttpStatusCode? StatusCode { get; set; }
    public IEnumerable<Error> Errors { get; set; } = [];
    public string? ErrorMsg { get; set; }

    /// <summary>
    /// 请求是否成功 (2xx 状态码)
    /// </summary>
    public bool IsSuccess => StatusCode.HasValue && (int)StatusCode.Value >= 200 && (int)StatusCode.Value < 300;

    /// <summary>
    /// 是否为未授权状态 (401)
    /// </summary>
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}

/// <summary>
/// API调用结果（带返回值场景）
/// </summary>
public class ApiResult<T> : ApiResult
{
    public T Data { get; set; } = default!;
}

public class Error(string code, string message)
{
    public string Code { get; set; } = code;
    public string Message { get; set; } = message;
}
