using System.Data;
using System.Reflection;
using Linger.Helper;

namespace Linger.Extensions.Data;

/// <summary>
/// Provides reflection-based <see cref="DataTable"/> object mapping methods.
/// </summary>
public static class DataTableReflectionExtensions
{
    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a <see cref="List{T}"/> by mapping public writable properties.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dataTable">The current <see cref="DataTable"/>.</param>
    /// <param name="parallelProcessingThreshold">The minimum number of rows required to enable parallel processing.</param>
    /// <returns>A <see cref="List{T}"/> representing the rows, or <see langword="null"/> when <paramref name="dataTable"/> is <see langword="null"/>.</returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map properties. Prefer the mapper overload in Linger.Utils for AOT/trimming scenarios.")]
#endif
    public static List<T>? ToList<T>(this DataTable? dataTable, int parallelProcessingThreshold = 1000)
        where T : class, new()
    {
        if (dataTable?.Rows.Count == 0)
        {
            return [];
        }

        if (dataTable is null)
        {
            return null;
        }

        var result = new List<T>(dataTable.Rows.Count);
        IReadOnlyDictionary<string, PropertyInfo> propertyMap = PropertyMetadataCache.GetPropertyMap(
            typeof(T),
            ignoreCase: true,
            writableOnly: true);

        var columnMappings = new Dictionary<int, PropertyInfo>();
        for (var i = 0; i < dataTable.Columns.Count; i++)
        {
            if (propertyMap.TryGetValue(dataTable.Columns[i].ColumnName, out PropertyInfo? property))
            {
                columnMappings[i] = property;
            }
        }

        if (columnMappings.Count == 0)
        {
            return result;
        }

        var useParallel = dataTable.Rows.Count > parallelProcessingThreshold;

        static T MapRow(DataRow row, IReadOnlyDictionary<int, PropertyInfo> mappings)
        {
            var item = new T();
            foreach (KeyValuePair<int, PropertyInfo> mapping in mappings)
            {
                var value = row[mapping.Key];
                if (value is not DBNull)
                {
                    SetProperty(item, mapping.Value, value);
                }
            }

            return item;
        }

        if (useParallel)
        {
            var items = new T[dataTable.Rows.Count];
            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                items[i] = MapRow(dataTable.Rows[i], columnMappings);
            });

            result.AddRange(items);
        }
        else
        {
            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(MapRow(row, columnMappings));
            }
        }

        return result;
    }

    private static void SetProperty<T>(T obj, PropertyInfo property, object? value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(property);

        if (!property.CanWrite)
        {
            throw new InvalidOperationException($"属性 {property.Name} 是只读的，无法赋值。");
        }

        if (value is null or DBNull)
        {
            if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
            {
                throw new InvalidCastException($"无法将 null 赋值给不可空的值类型属性: '{property.DeclaringType?.Name}.{property.Name}'。");
            }

            property.SetValue(obj, null);
            return;
        }

        if (!TypeConverter.TryConvert(value, property.PropertyType, out object? convertedValue))
        {
            throw new InvalidCastException(
                $"[核心转换失败] 无法将输入值 '{value}' (类型: {value.GetType().Name}) 转换为属性 '{property.Name}' 所需的目标类型 {property.PropertyType.Name}。 " +
                $"当前系统的运行时 Culture 是: '{CultureInfo.CurrentCulture.Name}'。");
        }

        if (convertedValue is null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
        {
            throw new InvalidCastException($"转换器发生异常：无法将转换后的 null 赋值给不可空值类型 '{property.Name}'。");
        }

        property.SetValue(obj, convertedValue);
    }
}
