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
    /// <returns>A <see cref="List{T}"/> representing the non-deleted rows, or <see langword="null"/> when <paramref name="dataTable"/> is <see langword="null"/>.</returns>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map properties. Prefer the mapper overload in Linger.Utils for AOT/trimming scenarios.")]
#endif
    public static List<T>? ToList<T>(this DataTable? dataTable)
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

        foreach (DataRow row in dataTable.Rows)
        {
            if (row.RowState != DataRowState.Deleted)
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
            throw new InvalidOperationException($"Property {property.Name} is read-only and cannot be assigned.");
        }

        if (value is null or DBNull)
        {
            if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
            {
                throw new InvalidCastException($"Cannot assign null to the non-nullable value-type property '{property.DeclaringType?.Name}.{property.Name}'.");
            }

            property.SetValue(obj, null);
            return;
        }

        if (!TypeConverter.TryConvert(value, property.PropertyType, out object? convertedValue))
        {
            throw new InvalidCastException(
                $"Cannot convert the value '{value}' (type {value.GetType().Name}) to the type {property.PropertyType.Name} required by property '{property.Name}'. " +
                "Provide a value the target type accepts directly, or convert it explicitly before mapping.");
        }

        if (convertedValue is null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
        {
            throw new InvalidCastException($"The converter returned null, which cannot be assigned to the non-nullable value-type property '{property.Name}'.");
        }

        property.SetValue(obj, convertedValue);
    }
}
