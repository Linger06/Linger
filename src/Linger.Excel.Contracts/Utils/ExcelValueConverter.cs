using System;
using System.Data;
using System.Globalization;

namespace Linger.Excel.Contracts.Utils
{
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
            if (value == null)
                return DBNull.Value;
                
            // 处理空字符串
            if (value is string s && string.IsNullOrEmpty(s))
                return DBNull.Value;
                
            // 处理日期类型
            if (isDateFormat)
            {
                if (value is DateTime dateTime)
                    return dateTime;
                    
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
            }
            
            // 数字类型处理 - 整数/小数识别
            if (value is double d)
            {
                if (Math.Abs(d - Math.Round(d)) < double.Epsilon)
                {
                    if (d >= int.MinValue && d <= int.MaxValue)
                        return (int)d;
                    return (long)d;
                }
                return d;
            }
            
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
                    return Convert.ToInt32(value);
                    
                if (actualType == typeof(long))
                    return Convert.ToInt64(value);
                    
                if (actualType == typeof(decimal))
                    return Convert.ToDecimal(value);
                    
                if (actualType == typeof(double))
                    return Convert.ToDouble(value);
                    
                if (actualType == typeof(float))
                    return Convert.ToSingle(value);
                    
                if (actualType == typeof(bool))
                {
                    if (value is string str)
                    {
                        str = str.Trim().ToLower();
                        return str == "true" || str == "yes" || str == "y" || str == "1";
                    }
                    return Convert.ToBoolean(value);
                }
                
                if (actualType == typeof(DateTime))
                {
                    if (value is double d)
                        return DateTime.FromOADate(d);
                        
                    if (value is string s)
                        return DateTime.Parse(s, CultureInfo.CurrentCulture);
                        
                    return Convert.ToDateTime(value);
                }
                
                if (actualType.IsEnum)
                {
                    if (value is string s)
                        return Enum.Parse(actualType, s, true);
                    
                    return Enum.ToObject(actualType, Convert.ToInt32(value));
                }
                
                // 其他类型尝试直接转换
                return Convert.ChangeType(value, actualType);
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
            string columnName = string.Empty;
            int dividend = columnIndex;
            int modulo;
            
            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
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
            int index = 0;
            
            for (int i = 0; i < columnName.Length; i++)
            {
                index *= 26;
                index += (columnName[i] - 'A' + 1);
            }
            
            return index;
        }
    }
}
