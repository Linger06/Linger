using System;
using System.Data;

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
    }
}
