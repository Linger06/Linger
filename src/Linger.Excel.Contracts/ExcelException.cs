namespace Linger.Excel.Contracts;

/// <summary>
/// Excel异常类
/// </summary>
public class ExcelException : Exception
{
    public ExcelException(string message) : base(message) { }

    public ExcelException(string message, Exception innerException) : base(message, innerException) { }
}
