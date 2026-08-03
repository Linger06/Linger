using Linger.Extensions.Core;

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
        foreach (DataRow sourceRow in source.Rows)
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
                throw new ArgumentException($"列 '{column.ColumnName}' 不存在于源数据表中");
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
        foreach (DataRow row in source.Rows)
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
        if (string.IsNullOrEmpty(captionName))
        {
            return;
        }

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
        if (!resultTable.Columns.Contains(captionName) || cellValue is DBNull)
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
            // 确保null和DBNull值也能生成有效的列名
            var stringValue = value?.ToString() ?? "NULL";

            // 替换可能导致列名无效的字符
            stringValue = SanitizeColumnName(stringValue);
            captionParts.Add(stringValue);
        }

        return string.Join("_", captionParts);
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
        IEnumerable<DataRow> rows = dt.AsEnumerable().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        foreach (DataRow item in rows)
        {
            result.ImportRow(item);
        }
        return result;
    }

}
