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
    public static List<T>? ToList<T>(this DataTable? dt) where T : class, new()
    {
        if (dt.IsNull())
        {
            return null;
        }

        if (dt.Rows.Count == 0)
        {
            return [];
        }

        var list = new List<T>();
        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (DataRow row in dt.Rows)
        {
            var model = new T();
            foreach (PropertyInfo? pi in properties)
            {
                var value = GetDataRowValue(pi.Name, pi.PropertyType, row);
                pi.SetValue(model, value, null);
            }
            list.Add(model);
        }
        return list;
    }

    /// <summary>
    /// Gets the value of the specified column in the <see cref="DataRow"/> based on the type.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="columnType">The type of the column.</param>
    /// <param name="dataRow">The <see cref="DataRow"/> to get the value from.</param>
    /// <returns>The value of the column.</returns>
    private static object? GetDataRowValue(string columnName, Type columnType, DataRow dataRow)
    {
        if (dataRow.Table.Columns.Contains(columnName))
        {
            var value = dataRow[columnName];
            return value.ConvertTo(columnType);
        }
        return null;
    }

    private static object? ConvertTo(this object value, Type type)
    {
        if (value == DBNull.Value) return null;

        return type switch
        {
            _ when type == typeof(string) => value.ToString(),
            _ when type == typeof(short) => Convert.ToInt16(value),
            _ when type == typeof(short?) => value.ToShortOrNull(),
            _ when type == typeof(int) => Convert.ToInt32(value),
            _ when type == typeof(int?) => value.ToIntOrNull(),
            _ when type == typeof(long) => Convert.ToInt64(value),
            _ when type == typeof(long?) => value.ToLongOrNull(),
            _ when type == typeof(float) => Convert.ToSingle(value),
            _ when type == typeof(float?) => value.ToFloatOrNull(),
            _ when type == typeof(double) => Convert.ToDouble(value),
            _ when type == typeof(double?) => value.ToDoubleOrNull(),
            _ when type == typeof(decimal) => Convert.ToDecimal(value),
            _ when type == typeof(decimal?) => value.ToDecimalOrNull(),
            _ when type == typeof(DateTime) => Convert.ToDateTime(value),
            _ when type == typeof(DateTime?) => value.ToDateTimeOrNull(),
            _ when type == typeof(bool) => Convert.ToBoolean(value),
            _ when type == typeof(bool?) => value.ToBoolOrNull(),
            _ when type == typeof(Guid) => value.ToGuid(),
            _ when type == typeof(Guid?) => value.ToGuidOrNull(),
            _ => value.ToString()
        };
    }
}