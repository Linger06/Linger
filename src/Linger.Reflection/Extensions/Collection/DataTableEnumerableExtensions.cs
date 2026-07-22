using System.Data;
using Linger.Extensions.Core;

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides reflection-based <see cref="DataTable"/> conversion methods for enumerable values.
/// </summary>
public static class DataTableEnumerableExtensions
{
    /// <summary>
    /// Converts an enumerable to a <see cref="DataTable"/> by reflecting over the element properties.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="recordList">The source enumerable.</param>
    /// <param name="actionColumn">An optional action to perform on each <see cref="DataColumn"/>.</param>
    /// <param name="actionRow">An optional action to perform on each <see cref="DataRow"/>.</param>
    /// <returns>A <see cref="DataTable"/> that contains the elements of the source enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="recordList"/> is null.</exception>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to discover properties. Use an explicit DataTable projection for AOT/trimming scenarios.")]
#endif
    public static DataTable ToDataTable<T>(
        this IEnumerable<T> recordList,
        Action<DataColumn, ColumnInfo>? actionColumn = null,
        Action<DataRow, ColumnInfo, T>? actionRow = null)
    {
        ArgumentNullException.ThrowIfNull(recordList);

        var columns = typeof(T).GetColumnsInfo();
        var dataTable = new DataTable();
        AddColumns(dataTable, columns, actionColumn);
        AddRows(dataTable, columns, recordList, actionRow);

        return dataTable;
    }

    private static void AddColumns(
        DataTable dataTable,
        IEnumerable<ColumnInfo> columns,
        Action<DataColumn, ColumnInfo>? action)
    {
        foreach (ColumnInfo column in columns)
        {
            var columnType = Nullable.GetUnderlyingType(column.Property.PropertyType) ?? column.Property.PropertyType;
            var dataColumn = new DataColumn(column.PropertyName, columnType);
            action?.Invoke(dataColumn, column);
            dataTable.Columns.Add(dataColumn);
        }
    }

    private static void AddRows<T>(
        DataTable dataTable,
        IEnumerable<ColumnInfo> columns,
        IEnumerable<T> values,
        Action<DataRow, ColumnInfo, T>? action)
    {
        foreach (T value in values)
        {
            DataRow row = dataTable.NewRow();
            foreach (ColumnInfo column in columns)
            {
                row[column.PropertyName] = column.Property.GetValue(value) ?? DBNull.Value;
                action?.Invoke(row, column, value);
            }

            dataTable.Rows.Add(row);
        }
    }
}
