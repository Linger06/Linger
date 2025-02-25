namespace Linger.Result;
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
