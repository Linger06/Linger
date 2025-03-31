namespace Linger.HttpClient.Contracts.Models;

/// <summary>
/// API调用结果
/// </summary>
public class ApiResult<T>
{
    public T Data { get; set; } = default!;
    public HttpStatusCode? StatusCode { get; set; }
    public ErrorObj? Errors { get; set; }
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
/// 错误对象
/// </summary>
public class ErrorObj
{
    public ErrorObj()
    {
        Form = new Dictionary<string, string>();
        Message = new List<string>();
    }

    public ErrorObj(string errorMsg) : this()
    {
        Message.Add(errorMsg);
    }

    public Dictionary<string, string> Form { get; set; } = default!;
    public List<string> Message { get; set; } = default!;
}