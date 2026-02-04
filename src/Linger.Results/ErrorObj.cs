namespace Linger.Results;

/// <summary>
///     错误对象（已过时，请使用 <see cref="Error"/> 代替）
/// </summary>
[Obsolete("Use Error instead. This type will be removed in a future version.")]
public class ErrorObj
{
    public ErrorObj()
    {
    }

    public ErrorObj(string errorMsg)
    {
        Message = [errorMsg];
    }

    public Dictionary<string, string> Form { get; set; } = [];
    public List<string> Message { get; set; } = [];
}
