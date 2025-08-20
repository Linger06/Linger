using System.Globalization;

namespace Linger.Excel.Contracts.Utils;

/// <summary>
/// Excel值转换工具
/// </summary>
public static class ExcelValueConverter
{
    /// <summary>
    /// 将Excel值转换为.NET数据类型
    /// </summary>
    /// <param name="value">Excel值</param>
    /// <param name="isDateFormat">是否为日期格式</param>
    /// <returns>转换后的值</returns>
    public static object ConvertToDbValue(object? value, bool isDateFormat = false)
    {
        // 空值处理
        if (value == null || value is DBNull)
            return DBNull.Value;

        // 处理空字符串
        if (value is string s && string.IsNullOrEmpty(s))
            return DBNull.Value;

        // 日期处理
        if (isDateFormat)
        {
            if (value is DateTime dateTime)
                return dateTime;

            // 处理OLE自动化日期（Excel中的数字日期）
            if (value is double numericDate)
            {
                try
                {
                    return DateTime.FromOADate(numericDate);
                }
                catch
                {
                    return value;
                }
            }

            // 从字符串转换日期
            if (value is string dateStr)
            {
                var dateValue = dateStr.ToDateTimeOrNull();
                if (dateValue.HasValue)
                    return dateValue.Value;
            }
        }

        // 数值类型处理
        if (value is double d)
        {
            if (Math.Abs(d - Math.Round(d)) < double.Epsilon)
            {
                if (d is >= int.MinValue and <= int.MaxValue)
                    return (int)d;
                return (long)d;
            }
            return d;
        }

        // 处理其他数值类型
        if (value is decimal m)
            return m;
        if (value is float f)
            return f;

        // 布尔值处理
        if (value is string boolStr)
        {
            var boolValue = boolStr.ToBoolOrNull();
            if (boolValue.HasValue)
                return boolValue.Value;
        }

        // GUID处理
        if (value is string guidStr)
        {
            var guidValue = guidStr.ToGuidOrNull();
            if (guidValue.HasValue)
                return guidValue.Value;
        }

        // 默认情况下返回原值
        return value;
    }

    /// <summary>
    /// 尝试将值转换为目标类型
    /// </summary>
    public static object? TryConvertValue(object? value, Type targetType)
    {
        if (value == null || value is DBNull)
            return null;

        // 获取Nullable的基础类型
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            // 处理不同的类型转换
            if (actualType == typeof(string))
                return value.ToString();

            if (actualType == typeof(int))
                return value.ToIntOrDefault();// Convert.ToInt32(value);

            if (actualType == typeof(long))
                return value.ToLongOrDefault();// Convert.ToInt64(value);

            if (actualType == typeof(decimal))
                return value.ToDecimalOrDefault();// Convert.ToDecimal(value);

            if (actualType == typeof(double))
                return value.ToDoubleOrDefault();// Convert.ToDouble(value);

            if (actualType == typeof(float))
                return value.ToFloatOrDefault();// Convert.ToSingle(value);

            if (actualType == typeof(bool))
            {
                //if (value is string str)
                //{
                //    str = str.Trim().ToLower();
                //    return str == "true" || str == "yes" || str == "y" || str == "1";
                //}
                return value.ToBoolOrDefault();// Convert.ToBoolean(value);
            }

            if (actualType == typeof(DateTime))
            {
                if (value is double d)
                    return DateTime.FromOADate(d);

                if (value is string s)
                    return DateTime.Parse(s, CultureInfo.CurrentCulture);

                return value.ToDateTimeOrDefault();// Convert.ToDateTime(value);
            }

            if (actualType.IsEnum)
            {
                if (value is string s)
                    return Enum.Parse(actualType, s, true);

                return Enum.ToObject(actualType, value.ToIntOrDefault());
            }

            // 其他类型尝试直接转换
            return Convert.ChangeType(value, actualType, ExtensionMethodSetting.DefaultCulture);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取Excel列名 (A, B, ..., Z, AA, AB, ...)
    /// </summary>
    public static string GetExcelColumnName(int columnIndex)
    {
        var columnName = string.Empty;
        var dividend = columnIndex;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    /// <summary>
    /// 将Excel列名转换为列索引
    /// </summary>
    public static int GetExcelColumnIndex(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return -1;

        columnName = columnName.ToUpperInvariant();
        var index = 0;

        for (var i = 0; i < columnName.Length; i++)
        {
            index *= 26;
            index += (columnName[i] - 'A' + 1);
        }

        return index;
    }
}
