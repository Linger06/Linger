using System;

namespace Linger.Excel.Utils
{
    /// <summary>
    /// Excel数据类型转换工具
    /// </summary>
    public static class ExcelTypeConverter
    {
        /// <summary>
        /// 将值转换为指定类型
        /// </summary>
        public static T? ConvertValue<T>(object? value)
        {
            if (value == null || value is DBNull)
                return default;
                
            Type targetType = typeof(T);
            
            try
            {
                // 处理Nullable类型
                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                
                // 处理特殊类型
                if (underlyingType == typeof(string))
                    return (T)(object)Convert.ToString(value)!;
                    
                if (underlyingType == typeof(int))
                    return (T)(object)Convert.ToInt32(value);
                    
                if (underlyingType == typeof(decimal))
                    return (T)(object)Convert.ToDecimal(value);
                    
                if (underlyingType == typeof(double))
                    return (T)(object)Convert.ToDouble(value);
                    
                if (underlyingType == typeof(DateTime))
                {
                    // 处理Excel日期格式
                    if (value is double numericDate)
                        return (T)(object)DateTime.FromOADate(numericDate);
                    return (T)(object)Convert.ToDateTime(value);
                }
                
                if (underlyingType == typeof(bool))
                {
                    if (value is string strValue)
                    {
                        strValue = strValue.ToLower();
                        return (T)(object)(strValue == "true" || strValue == "yes" || strValue == "y" || strValue == "1");
                    }
                    return (T)(object)Convert.ToBoolean(value);
                }
                
                // 处理枚举类型
                if (underlyingType.IsEnum)
                {
                    if (value is string strValue)
                        return (T)Enum.Parse(underlyingType, strValue);
                    
                    if (value is int intValue)
                        return (T)Enum.ToObject(underlyingType, intValue);
                }
                
                // 其他类型尝试直接转换
                return (T)Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                // 转换失败返回默认值
                return default;
            }
        }
    }
}
