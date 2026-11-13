using Linger.Extensions.Core;
using System.Globalization;

namespace Linger.Extensions.Data;

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
    /// <remarks>
    /// 忽略 Deleted 行。标题使用固定区域性格式；空字符串生成 EMPTY，空值生成 NULL。
    /// 不同标题生成同名列时抛出 DuplicateNameException。分组键按序数、区分大小写比较。
    /// </remarks>
    public static DataTable TableRowTurnToColumn(this DataTable source, DataColumn[] groupColumns,
        DataColumn[] captionColumns, DataColumn valueColumn)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(groupColumns, nameof(groupColumns));
        ArgumentNullException.ThrowIfNull(captionColumns, nameof(captionColumns));
        ArgumentNullException.ThrowIfNull(valueColumn, nameof(valueColumn));

        ValidatePivotColumns(source, groupColumns, captionColumns, valueColumn);
        var resultTable = CreatePivotResultTable(source, groupColumns);

        // 如果源表没有数据，直接返回只包含列定义的空表
        if (source.Rows.Count == 0)
        {
            return resultTable;
        }

        AddPivotCaptionColumns(source, resultTable, captionColumns);

        // 使用字典存储分组键与其对应的数据行
        var groupedRows = new Dictionary<object?[], DataRow>(ColumnValueArrayComparer.Instance);

        // 分组处理数据
        foreach (var sourceRow in EnumerateCurrentRows(source))
        {
            var resultRow = GetOrCreatePivotRow(sourceRow, resultTable, groupedRows, groupColumns);
            AccumulatePivotValue(sourceRow, resultRow, resultTable, captionColumns, valueColumn);
        }

        return resultTable;
    }

    private static void ValidatePivotColumns(
        DataTable source,
        DataColumn[] groupColumns,
        DataColumn[] captionColumns,
        DataColumn valueColumn)
    {
        foreach (var column in groupColumns.Concat(captionColumns).Append(valueColumn))
        {
            if (!source.Columns.Contains(column.ColumnName))
            {
                throw new ArgumentException($"Column '{column.ColumnName}' does not exist in the source table.");
            }
        }
    }

    private static DataTable CreatePivotResultTable(DataTable source, DataColumn[] groupColumns)
    {
        var resultTable = new DataTable();
        foreach (var groupColumn in groupColumns)
        {
            var sourceColumn = source.Columns[groupColumn.ColumnName];
            ArgumentNullException.ThrowIfNull(sourceColumn);
            resultTable.Columns.Add(sourceColumn.ColumnName, sourceColumn.DataType);
        }

        return resultTable;
    }

    private static void AddPivotCaptionColumns(
        DataTable source,
        DataTable resultTable,
        DataColumn[] captionColumns)
    {
        var captionValuesByName = GetPivotCaptionValues(source, captionColumns);
        foreach (var captionName in captionValuesByName.Keys)
        {
            AddPivotCaptionColumn(resultTable, captionName);
        }
    }

    private static Dictionary<string, object?[]> GetPivotCaptionValues(
        DataTable source,
        DataColumn[] captionColumns)
    {
        var captionValuesByName = new Dictionary<string, object?[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in EnumerateCurrentRows(source))
        {
            var captionValues = GetColumnValues(row, captionColumns);
            var captionName = FormatCaptionValue(captionValues);

            if (!captionValuesByName.TryGetValue(captionName, out var existingCaptionValues))
            {
                captionValuesByName.Add(captionName, captionValues);
                continue;
            }

            if (!ColumnValueArrayComparer.Instance.Equals(existingCaptionValues, captionValues))
            {
                throw new DuplicateNameException(captionName);
            }
        }

        return captionValuesByName;
    }

    private static void AddPivotCaptionColumn(DataTable resultTable, string captionName)
    {
        if (resultTable.Columns.Contains(captionName))
        {
            throw new DuplicateNameException(captionName);
        }

        var newColumn = resultTable.Columns.Add(captionName, typeof(decimal));
        newColumn.AllowDBNull = true;
    }

    private static DataRow GetOrCreatePivotRow(
        DataRow sourceRow,
        DataTable resultTable,
        Dictionary<object?[], DataRow> groupedRows,
        DataColumn[] groupColumns)
    {
        var groupKey = GetColumnValues(sourceRow, groupColumns);
        if (groupedRows.TryGetValue(groupKey, out var resultRow))
        {
            return resultRow;
        }

        resultRow = resultTable.NewRow();
        foreach (var groupColumn in groupColumns)
        {
            resultRow[groupColumn.ColumnName] = sourceRow[groupColumn.ColumnName];
        }

        resultTable.Rows.Add(resultRow);
        groupedRows.Add(groupKey, resultRow);

        return resultRow;
    }

    private static void AccumulatePivotValue(
        DataRow sourceRow,
        DataRow resultRow,
        DataTable resultTable,
        DataColumn[] captionColumns,
        DataColumn valueColumn)
    {
        var captionName = FormatCaptionValue(GetColumnValues(sourceRow, captionColumns));
        var cellValue = sourceRow[valueColumn.ColumnName];
        if (cellValue is DBNull)
        {
            return;
        }

        var currentValue = resultRow[captionName] is DBNull
            ? 0
            : resultRow[captionName].ToDecimal();
        resultRow[captionName] = currentValue + cellValue.ToDecimal();
    }

    private static object?[] GetColumnValues(DataRow row, DataColumn[] columns)
    {
        var values = new object?[columns.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            var value = row[columns[i].ColumnName];
            values[i] = value is DBNull ? null : value;
        }

        return values;
    }

    // 格式化列标题值
    private static string FormatCaptionValue(object?[] captionValues)
    {
        var captionParts = new List<string>();
        foreach (var value in captionValues)
        {
            // 固定列名格式，避免当前区域性改变输出表结构。
            var stringValue = value is null ? "NULL" : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            if (stringValue.Length == 0)
            {
                stringValue = "EMPTY";
            }

            // 替换可能导致列名无效的字符
            stringValue = SanitizeColumnName(stringValue);
            captionParts.Add(stringValue);
        }

        // 没有标题列时仍需承接聚合值，不能因空列名静默丢弃。
        return captionParts.Count == 0 ? "EMPTY" : string.Join("_", captionParts);
    }

    // 净化列名，替换无效字符
    private static string SanitizeColumnName(string columnName)
    {
        // 替换常见的无效列名字符
        return columnName
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "_")
            .Replace("*", "_")
            .Replace("?", "_")
            .Replace("\"", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("|", "_")
            .Replace(" ", "_");
    }

    private sealed class ColumnValueArrayComparer : IEqualityComparer<object?[]>
    {
        public static ColumnValueArrayComparer Instance { get; } = new();

        public bool Equals(object?[]? x, object?[]? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object?[] obj)
        {
            unchecked
            {
                var hashCode = 17;
                foreach (var value in obj)
                {
                    hashCode = (hashCode * 31) + (value is null
                        ? 0
                        : StructuralComparisons.StructuralEqualityComparer.GetHashCode(value));
                }

                return hashCode;
            }
        }
    }

    /// <summary>
    /// Paginates the <see cref="DataTable"/>.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to paginate.</param>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The number of rows per page.</param>
    /// <returns>A new <see cref="DataTable"/> containing the paginated rows.</returns>
    /// <remarks>Deleted 行不计入页容量；不改变源表的行状态。</remarks>
    public static DataTable? Paging(this DataTable? dt, int pageIndex, int pageSize)
    {
        if (dt.IsNull())
        {
            return null;
        }
        if (dt.Rows.Count == 0 || pageIndex <= 0 || pageSize <= 0)
        {
            return dt.Clone();
        }

        DataTable result = dt.Clone();
        long offset = ((long)pageIndex - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return result;
        }

        IEnumerable<DataRow> rows = EnumerateCurrentRows(dt).Skip((int)offset).Take(pageSize);
        foreach (DataRow item in rows)
        {
            result.ImportRow(item);
        }

        return result;
    }

}
