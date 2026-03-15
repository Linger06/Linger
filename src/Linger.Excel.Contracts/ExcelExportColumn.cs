namespace Linger.Excel.Contracts;

/// <summary>
/// 定义一个用于无反射 Excel 导出的显式列。
/// </summary>
/// <typeparam name="T">源数据类型。</typeparam>
public sealed class ExcelExportColumn<T> : ExcelColumnInfo
{
    /// <summary>
    /// 初始化 <see cref="ExcelExportColumn{T}"/> 的新实例。
    /// </summary>
    /// <param name="header">导出列头。</param>
    /// <param name="valueSelector">从源对象中选择单元格值的委托。</param>
    /// <param name="dataType">目标列的数据类型。为 <c>null</c> 时使用 <see cref="object"/>。</param>
    public ExcelExportColumn(string header, Func<T, object?> valueSelector, Type? dataType = null)
        : this(
            new ExcelColumnInfo
            {
                PropertyName = header,
                DisplayName = header,
                PropertyType = NormalizeDataType(dataType),
                Order = int.MaxValue
            },
            valueSelector)
    {
    }

    /// <summary>
    /// 使用现有 <see cref="ExcelColumnInfo"/> 元数据初始化 <see cref="ExcelExportColumn{T}"/>。
    /// </summary>
    /// <param name="columnInfo">可复用的列元数据。</param>
    /// <param name="valueSelector">从源对象中选择单元格值的委托。</param>
    public ExcelExportColumn(ExcelColumnInfo columnInfo, Func<T, object?> valueSelector)
    {
        ArgumentNullException.ThrowIfNull(columnInfo);
        ArgumentNullException.ThrowIfNull(valueSelector);

        if (string.IsNullOrWhiteSpace(columnInfo.DisplayName))
        {
            throw new ArgumentException("DisplayName cannot be null or whitespace.", nameof(columnInfo));
        }

        ArgumentNullException.ThrowIfNull(columnInfo.PropertyType);

        PropertyName = string.IsNullOrWhiteSpace(columnInfo.PropertyName) ? columnInfo.DisplayName : columnInfo.PropertyName;
        DisplayName = columnInfo.DisplayName;
        Required = columnInfo.Required;
        PropertyType = NormalizeDataType(columnInfo.PropertyType);
        Order = columnInfo.Order;
        ValueSelector = valueSelector;
    }

    /// <summary>
    /// 导出列头。
    /// </summary>
    public string Header => DisplayName;

    /// <summary>
    /// 从源对象中选择单元格值的委托。
    /// </summary>
    public Func<T, object?> ValueSelector { get; }

    /// <summary>
    /// DataTable 列类型。
    /// </summary>
    public Type DataType => PropertyType;

    private static Type NormalizeDataType(Type? dataType)
    {
        if (dataType is null)
        {
            return typeof(object);
        }

        return Nullable.GetUnderlyingType(dataType) ?? dataType;
    }
}
