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

    public Dictionary<string, string> Form { get; set; } = new();
    public List<string> Message { get; set; } = new();
}
