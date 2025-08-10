using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Linger.Excel.Contracts.Attributes;

namespace Linger.Excel.Contracts.Utils;

/// <summary>
/// Excel模板生成器
/// </summary>
public static class ExcelTemplateGenerator
{
    /// <summary>
    /// 创建列信息模型
    /// </summary>
    public static List<ExcelColumnInfo> CreateColumnInfos<T>() where T : class
    {
        var result = new List<ExcelColumnInfo>();
        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            if (!property.CanRead) continue;

            // Check if the property has the ExcelColumn attribute
            var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
            if (excelColumnAttribute == null) continue;

            var columnInfo = new ExcelColumnInfo
            {
                PropertyName = property.Name,
                DisplayName = GetDisplayName(property),
                Required = IsRequired(property),
                PropertyType = property.PropertyType,
                Order = GetColumnOrder(property)
            };

            result.Add(columnInfo);
        }

        return result.OrderBy(x => x.Order).ToList();
    }

    private static string GetDisplayName(PropertyInfo property)
    {
        // 优先使用DisplayAttribute
        var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
            return displayAttribute.Name;

        // 其次使用DisplayNameAttribute
        var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
            return displayNameAttribute.DisplayName;

        // 最后尝试使用ExcelColumnAttribute
        var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
        if (excelColumnAttribute != null && excelColumnAttribute.ColumnName.IsNotNullOrEmpty())
            return excelColumnAttribute.ColumnName;

        // 默认返回属性名
        return property.Name;
    }

    private static bool IsRequired(PropertyInfo property)
    {
        return property.GetCustomAttribute<RequiredAttribute>() != null;
    }

    private static int GetColumnOrder(PropertyInfo property)
    {
        // 优先使用DisplayAttribute的Order
        var columnOrderAttribute = property.GetCustomAttribute<DisplayAttribute>();
        if (columnOrderAttribute != null)
        {
            var order = columnOrderAttribute.GetOrder();
            if (order.HasValue)
                return order.Value;
        }

        // 其次使用ExcelColumnAttribute的Index
        var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
        if (excelColumnAttribute != null && excelColumnAttribute.Index != int.MaxValue)
            return excelColumnAttribute.Index;

        return int.MaxValue;
    }
}

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
