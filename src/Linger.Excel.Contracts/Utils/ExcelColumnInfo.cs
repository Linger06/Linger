namespace Linger.Excel.Contracts.Utils;

/// <summary>
/// Excel列信息
/// </summary>
public class ExcelColumnInfo
{
    public string PropertyName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool Required { get; set; }
    public Type PropertyType { get; set; } = null!;
    public int Order { get; set; }
}