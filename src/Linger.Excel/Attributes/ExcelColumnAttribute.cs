namespace Linger.Excel.Attributes;

/// <summary>
///     Excel 自动导出时，指定属性的列名
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ExcelColumnAttribute : System.Attribute
{
    #region 公开属性

    /// <summary>
    ///     列名
    /// </summary>
    public string? ColumnName { set; get; }

    /// <summary>
    ///     索引
    /// </summary>
    public int Index { set; get; }

    #endregion 公开属性
}