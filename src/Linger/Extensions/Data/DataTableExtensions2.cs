using System.Reflection;
using Linger.Extensions.Core;

#if !NETFRAMEWORK || NET462_OR_GREATER
using Linger.JsonConverter;
using System.Text.Json;
#endif

namespace Linger.Extensions.Data;

/// <summary>
/// Extensions for <see cref="DataTable"/>.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts rows to columns in the <see cref="DataTable"/>.
    /// </summary>
    /// <param name="source">The source <see cref="DataTable"/>.</param>
    /// <param name="groupColumns">The columns to group by.</param>
    /// <param name="captionColumns">The columns to use as new column names.</param>
    /// <param name="valueColumn">The column containing the values.</param>
    /// <returns>A new <see cref="DataTable"/> with rows converted to columns.</returns>
    public static DataTable TableRowTurnToColumn(this DataTable source, DataColumn[] groupColumns,
        DataColumn[] captionColumns, DataColumn valueColumn)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(groupColumns, nameof(groupColumns));
        ArgumentNullException.ThrowIfNull(captionColumns, nameof(captionColumns));

        var dt = new DataTable();
        foreach (DataColumn item in groupColumns)
        {
            dt.Columns.Add(item.ColumnName, item.DataType);
        }

        IEnumerable<string> newColumnNames = source.AsEnumerable()
            .Select(p => string.Join("_", captionColumns.Select(q => p[q])))
            .Distinct();

        foreach (var item in newColumnNames)
        {
            if (!dt.Columns.Contains(item))
            {
                dt.Columns.Add(item);
            }
        }

        var groupData = source.AsEnumerable()
            .GroupBy(p => string.Join(string.Empty, groupColumns.Select(q => p[q])))
            .ToList();

        var groupColNames = new HashSet<string>(groupColumns.Select(p => p.ColumnName));
        foreach (IGrouping<string, DataRow>? x in groupData)
        {
            DataRow newRow = dt.NewRow();
            foreach (DataColumn col in dt.Columns)
            {
                if (groupColNames.Contains(col.ColumnName))
                {
                    newRow[col.ColumnName] = x.FirstOrDefault()![col.ColumnName];
                }
                else
                {
                    IEnumerable<DataRow> data = x.Where(p => string.Join("_", captionColumns.Select(q => p[q])) == col.ColumnName);
                    if (data.Any())
                    {
                        newRow[col.ColumnName] = data.Sum(p => p[valueColumn].ToDecimal());
                    }
                }
            }
            dt.Rows.Add(newRow);
        }
        return dt;
    }

    /// <summary>
    /// Paginates the <see cref="DataTable"/>.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to paginate.</param>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The number of rows per page.</param>
    /// <returns>A new <see cref="DataTable"/> containing the paginated rows.</returns>
    public static DataTable? Paging(this DataTable? dt, int pageIndex, int pageSize)
    {
        if (dt.IsNull())
        {
            return null;
        }
        if (dt.Rows.Count == 0)
        {
            return dt;
        }
        DataTable result = dt.Clone();
        IEnumerable<DataRow> rows = dt.AsEnumerable().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        foreach (DataRow? item in rows)
        {
            result.ImportRow(item);
        }
        return result;
    }


#if !NETFRAMEWORK || NET462_OR_GREATER
    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a JSON string.
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable"/> to convert.</param>
    /// <returns>A JSON string representing the <see cref="DataTable"/>.</returns>
    public static string ToJsonString(this DataTable? dataTable)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new DataTableJsonConverter(), new DateTimeConverter() }
        };

        return dataTable.ToJsonString(options);
    }
#endif

    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dt">The current <see cref="DataTable"/>.</param>
    /// <returns>A <see cref="List{T}"/> representing the rows.</returns>
    public static List<T>? ToList<T>(this DataTable? dataTable, int parallelProcessingThreshold = 1000) where T : class, new()
    {
        if (dataTable.IsNull())
        {
            return null;
        }

        var result = new List<T>(dataTable.Rows.Count);
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanWrite)
            .ToArray();

        if (properties.Length == 0)
        {
            //Logger?.LogWarning("类型 {Type} 没有可写属性", typeof(T).Name);
            return result;
        }

        // 创建列名到属性的映射（不区分大小写）
        var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in properties)
        {
            propertyMap[prop.Name] = prop;
        }

        // 创建列索引到属性的映射
        var columnMappings = new Dictionary<int, PropertyInfo>();
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            if (propertyMap.TryGetValue(dataTable.Columns[i].ColumnName, out var property))
            {
                columnMappings[i] = property;
            }
        }

        if (columnMappings.Count == 0)
        {
            //Logger?.LogWarning("未找到任何列名与类型 {Type} 的属性匹配", typeof(T).Name);
            return result;
        }

        // 判断是否使用并行处理
        bool useParallel = dataTable.Rows.Count > parallelProcessingThreshold;

        if (useParallel)
        {
            //Logger?.LogDebug("使用并行处理转换 {Count} 行数据为对象列表", dataTable.Rows.Count);

            var items = new T[dataTable.Rows.Count];

            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                var item = new T();
                var row = dataTable.Rows[i];

                foreach (var mapping in columnMappings)
                {
                    int colIndex = mapping.Key;
                    PropertyInfo property = mapping.Value;
                    var value = row[colIndex];

                    if (value != DBNull.Value)
                    {
                        SetPropertySafely(item, property, value);
                    }
                }

                items[i] = item;
            });

            result.AddRange(items);
        }
        else
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var item = new T();

                foreach (var mapping in columnMappings)
                {
                    int colIndex = mapping.Key;
                    PropertyInfo property = mapping.Value;
                    var value = row[colIndex];

                    if (value != DBNull.Value)
                    {
                        SetPropertySafely(item, property, value);
                    }
                }

                result.Add(item);
            }
        }

        return result;
    }

    private static void SetPropertySafely<T>(T obj, PropertyInfo property, object? value) where T : class
    {
        if (value == null || value is DBNull)
            return;

        try
        {
            // 处理Nullable类型
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // 特殊类型处理
            if (propertyType == typeof(string))
            {
                property.SetValue(obj, value.ToString());
                return;
            }

            if (propertyType == typeof(DateTime))
            {
                if (value is DateTime dateTime)
                {
                    property.SetValue(obj, dateTime);
                }
                else if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                {
                    property.SetValue(obj, parsedDate);
                }
                else if (value is double numericDate)
                {
                    property.SetValue(obj, DateTime.FromOADate(numericDate));
                }
                return;
            }

            if (propertyType == typeof(bool))
            {
                if (value is bool boolValue)
                {
                    property.SetValue(obj, boolValue);
                }
                else
                {
                    string strValue = value.ToString()!.ToLower();
                    property.SetValue(obj, strValue == "true" || strValue == "yes" || strValue == "y" || strValue == "1");
                }
                return;
            }

            if (propertyType.IsEnum)
            {
                if (value is string strValue)
                {
                    try
                    {
                        var enumValue = Enum.Parse(propertyType, strValue, true);
                        property.SetValue(obj, enumValue);
                        return;
                    }
                    catch
                    {
                        // Parsing failed, continue with other conversion attempts
                    }
                }
                if (int.TryParse(value.ToString(), out int intValue))
                {
                    property.SetValue(obj, Enum.ToObject(propertyType, intValue));
                    return;
                }
            }

            // 常规类型转换
            property.SetValue(obj, Convert.ChangeType(value, propertyType));
        }
        catch (Exception)
        {
            // 转换失败记录日志
            //Logger?.LogDebug(ex, "属性设置失败: {PropertyName}, 值: {Value}, 值类型: {ValueType}", property.Name, value, value.GetType().Name);
            throw;
        }
    }
}