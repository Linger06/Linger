using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Linger.Excel.Contracts.Attributes;

namespace Linger.Excel.Contracts.Utils
{
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
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
                return displayAttribute.Name;
                
            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
                return displayNameAttribute.DisplayName;
                
            var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
            if (excelColumnAttribute != null && !string.IsNullOrEmpty(excelColumnAttribute.ColumnName))
                return excelColumnAttribute.ColumnName;
                
            return property.Name;
        }
        
        private static bool IsRequired(PropertyInfo property)
        {
            return property.GetCustomAttribute<RequiredAttribute>() != null;
        }
        
        private static int GetColumnOrder(PropertyInfo property)
        {
            var columnOrderAttribute = property.GetCustomAttribute<DisplayAttribute>();
            if (columnOrderAttribute != null && columnOrderAttribute.Order >= 0)
                return columnOrderAttribute.Order;
                
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
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public bool Required { get; set; }
        public Type PropertyType { get; set; }
        public int Order { get; set; }
    }
}
