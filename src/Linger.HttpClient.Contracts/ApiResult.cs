using System.Net;

namespace Linger.HttpClient.Contracts;
public class ApiResult<T>
{
    public T Data { get; set; } = default!;
    public HttpStatusCode? StatusCode { get; set; }
    public ErrorObj? Errors { get; set; }
    public string? ErrorMsg { get; set; }
}

public class ErrorObj
{
    public ErrorObj()
    {
    }

    public ErrorObj(string errorMsg)
    {
        Message = [errorMsg];
    }

    public Dictionary<string, string> Form { get; set; } = default!;
    public List<string> Message { get; set; } = default!;
}