using System.Collections;
using Linger.Extensions.Core;

namespace Linger.Extensions.Data;

/// <summary>
/// Extensions for <see cref="DataTable"/> with performance optimizations.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Removes empty rows from the current <see cref="DataTable"/>.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to remove empty rows from.</param>
    /// <returns>The <see cref="DataTable"/> with empty rows removed.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// table = table.ClearEmptyRow();
    /// </code>
    /// </example>
    public static DataTable? ClearEmptyRow(this DataTable? dt)
    {
        if (dt.IsNull())
        {
            return null;
        }

        for (var i = dt.Rows.Count - 1; i >= 0; i--)
        {
            var emptyColumnCount = 0;
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var item = dt.Rows[i][j].ToString();
                if (!string.IsNullOrEmpty(item))
                {
                    break;
                }

                emptyColumnCount++;
            }

            if (emptyColumnCount == dt.Columns.Count)
            {
                dt.Rows.RemoveAt(i);
            }
        }

        return dt;
    }

    /// <summary>
    /// Finds rows in the <see cref="DataTable"/> that match the specified condition.
    /// </summary>
    /// <param name="sourceTable">The <see cref="DataTable"/> to search.</param>
    /// <param name="condition">The condition to match, e.g., "a&gt;1 and b&gt;3".</param>
    /// <returns>A <see cref="DataTable"/> containing the matching rows.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// DataTable result = table.Find("ColumnName = 'Value'");
    /// </code>
    /// </example>
    public static DataTable Find(this DataTable sourceTable, string condition)
    {
        DataRow[] foundRows = sourceTable.Select(condition);

        DataTable temp = sourceTable.Clone();
        foreach (DataRow dr in foundRows)
        {
            _ = temp.Rows.Add(dr.ItemArray);
        }

        return temp;
    }

    /// <summary>
    /// Sorts the <see cref="DataTable"/> based on the specified sorting expression.
    /// </summary>
    /// <param name="sourceTable">The <see cref="DataTable"/> to sort.</param>
    /// <param name="sortingExpression">The sorting expression, e.g., "Id desc".</param>
    /// <returns>A sorted <see cref="DataTable"/>.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// DataTable sortedTable = table.Sort("ColumnName ASC");
    /// </code>
    /// </example>
    public static DataTable Sort(this DataTable sourceTable, string sortingExpression)
    {
        var view = new DataView(sourceTable) { Sort = sortingExpression };
        return view.ToTable();
    }

    /// <summary>
    /// Removes duplicate rows from the <see cref="DataTable"/> based on the specified columns.
    /// </summary>
    /// <param name="sourceTable">The <see cref="DataTable"/> to remove duplicates from.</param>
    /// <param name="distinctColumns">The columns to consider for duplicates, e.g., new string[] { "Item", "ddd" }.</param>
    /// <returns>A <see cref="DataTable"/> with duplicates removed.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// DataTable distinctTable = table.Distinct(new string[] { "ColumnName1", "ColumnName2" });
    /// </code>
    /// </example>
    public static DataTable Distinct(this DataTable sourceTable, string[] distinctColumns)
    {
        return new DataView(sourceTable).ToTable(true, distinctColumns);
    }

    /// <summary>
    /// Sums the values of the specified column in the <see cref="DataTable"/>.
    /// </summary>
    /// <param name="sourceTable">The <see cref="DataTable"/> to sum values from.</param>
    /// <param name="columnName">The name of the column to sum.</param>
    /// <returns>The sum of the values in the specified column.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// double sum = table.Sum("ColumnName");
    /// </code>
    /// </example>
    public static double Sum(this DataTable sourceTable, string columnName)
    {
        ArgumentNullException.ThrowIfNull(sourceTable);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        return sourceTable.AsEnumerable().Sum(dr => dr[columnName].ToDoubleOrDefault());
    }

    /// <summary>
    /// Combines two <see cref="DataTable"/> objects with the same structure.
    /// </summary>
    /// <param name="dataTable1">The first <see cref="DataTable"/>.</param>
    /// <param name="dataTable2">The second <see cref="DataTable"/>.</param>
    /// <returns>A new <see cref="DataTable"/> that contains the combined rows of both tables.</returns>
    /// <example>
    /// <code>
    /// DataTable table1 = GetDataTable1();
    /// DataTable table2 = GetDataTable2();
    /// DataTable combinedTable = table1.Combine(table2);
    /// </code>
    /// </example>
    public static DataTable Combine(this DataTable dataTable1, DataTable dataTable2)
    {
        DataTable newDataTable = dataTable1.Clone();

        var obj = new object[newDataTable.Columns.Count];
        for (var i = 0; i < dataTable1.Rows.Count; i++)
        {
            dataTable1.Rows[i].ItemArray.CopyTo(obj, 0);
            _ = newDataTable.Rows.Add(obj);
        }

        for (var i = 0; i < dataTable2.Rows.Count; i++)
        {
            dataTable2.Rows[i].ItemArray.CopyTo(obj, 0);
            _ = newDataTable.Rows.Add(obj);
        }

        return newDataTable;
    }

    /// <summary>
    /// Determines whether the <see cref="DataTable"/> contains all specified columns.
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable"/> to check.</param>
    /// <param name="columnString">A comma-separated string of column names to check for.</param>
    /// <returns><c>true</c> if the <see cref="DataTable"/> contains all specified columns; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// bool containsAllColumns = table.ContainAllColumns("ColumnName1,ColumnName2");
    /// </code>
    /// </example>
    public static bool ContainAllColumns(this DataTable dataTable, string columnString)
    {
        var array = columnString.Split(',');
        var flag = true;
        foreach (var item in array)
        {
            if (!dataTable.Columns.Contains(item))
            {
                flag = false;
                break;
            }
        }

        return flag;
    }

    /// <summary>
    /// Joins two <see cref="DataTable"/> objects based on the specified columns.
    /// </summary>
    /// <param name="left">The left <see cref="DataTable"/>.</param>
    /// <param name="right">The right <see cref="DataTable"/>.</param>
    /// <param name="leftCols">The columns from the left table to join on.</param>
    /// <param name="rightCols">The columns from the right table to join on.</param>
    /// <param name="includeLeftJoin">Whether to include all rows from the left table.</param>
    /// <param name="includeRightJoin">Whether to include all rows from the right table.</param>
    /// <returns>
    /// <para>A new <see cref="DataTable"/> that contains the joined rows.</para>
    /// Left Join: <paramref name="includeLeftJoin"/>  true, <paramref name="includeRightJoin"/>  false. <br/>
    /// Right Join: <paramref name="includeLeftJoin"/>  false, <paramref name="includeRightJoin"/>  true. <br/>
    /// Inner Join: <paramref name="includeLeftJoin"/>  false, <paramref name="includeRightJoin"/>  false. <br/>
    /// Full Outter Join: <paramref name="includeLeftJoin"/>  true, <paramref name="includeRightJoin"/>  true. <br/>
    /// </returns>
    /// <example>
    /// <code>
    /// DataTable table1 = GetDataTable1();
    /// DataTable table2 = GetDataTable2();
    /// DataTable joinedTable = table1.Join(table2, new DataColumn[] { table1.Columns["Id"] }, new DataColumn[] { table2.Columns["Id"] }, true, false);
    /// </code>
    /// </example>
    /// <exception cref="ArgumentException">Thrown when the specified columns are not found in the respective tables.</exception>
    public static DataTable Join(this DataTable left, DataTable right, DataColumn[] leftCols, DataColumn[] rightCols,
        bool includeLeftJoin, bool includeRightJoin)
    {
        ValidateJoinColumns(left, leftCols, nameof(leftCols), nameof(left));
        ValidateJoinColumns(right, rightCols, nameof(rightCols), nameof(right));

        using DataSet ds = new();
        ds.Tables.AddRange([left.Copy(), right.Copy()]);
        var leftRelationCols = ResolveJoinColumns(ds.Tables[0], leftCols);
        var rightRelationCols = ResolveJoinColumns(ds.Tables[1], rightCols);
        var result = CreateJoinResultTable(left, right);

        DataRelation leftRelation = new("rLeft", leftRelationCols, rightRelationCols, false);
        ds.Relations.Add(leftRelation);

        result.BeginLoadData();
        LoadLeftJoinRows(result, ds.Tables[0], leftRelation, includeLeftJoin);

        if (includeRightJoin)
        {
            DataRelation rightRelation = new("rRight", rightRelationCols, leftRelationCols, false);
            ds.Relations.Add(rightRelation);
            LoadUnmatchedRightRows(result, ds.Tables[1], rightRelation);
        }

        result.EndLoadData();

        return result;
    }

    private static void ValidateJoinColumns(
        DataTable table,
        DataColumn[] columns,
        string columnsParameterName,
        string tableParameterName)
    {
        foreach (var column in columns)
        {
            if (!table.ContainAllColumns(column.ColumnName))
            {
                throw new ArgumentException($"{columnsParameterName} have columns not in {tableParameterName}");
            }
        }
    }

    private static DataColumn[] ResolveJoinColumns(DataTable table, DataColumn[] sourceColumns)
    {
        var result = new DataColumn[sourceColumns.Length];
        for (var i = 0; i < sourceColumns.Length; i++)
        {
            result[i] = table.Columns[sourceColumns[i].ColumnName]!;
        }

        return result;
    }

    private static DataTable CreateJoinResultTable(DataTable left, DataTable right)
    {
        DataTable result = new("JoinResult");
        foreach (DataColumn column in left.Columns)
        {
            result.Columns.Add(column.ColumnName, column.DataType);
        }

        foreach (DataColumn column in right.Columns)
        {
            var columnName = column.ColumnName;
            while (result.Columns.Contains(columnName))
            {
                columnName += "_2";
            }

            result.Columns.Add(columnName, column.DataType);
        }

        return result;
    }

    private static void LoadLeftJoinRows(
        DataTable result,
        DataTable left,
        DataRelation relation,
        bool includeUnmatchedRows)
    {
        foreach (DataRow leftRow in left.Rows)
        {
            var rightRows = leftRow.GetChildRows(relation);
            if (rightRows.Length == 0)
            {
                if (includeUnmatchedRows)
                {
                    LoadUnmatchedLeftRow(result, leftRow);
                }

                continue;
            }

            foreach (var rightRow in rightRows)
            {
                LoadMatchedJoinRow(result, leftRow, rightRow);
            }
        }
    }

    private static void LoadMatchedJoinRow(DataTable result, DataRow leftRow, DataRow rightRow)
    {
        var leftValues = leftRow.ItemArray;
        var rightValues = rightRow.ItemArray;
        var joinedValues = new object[leftValues.Length + rightValues.Length];
        Array.Copy(leftValues, 0, joinedValues, 0, leftValues.Length);
        Array.Copy(rightValues, 0, joinedValues, leftValues.Length, rightValues.Length);
        result.LoadDataRow(joinedValues, true);
    }

    private static void LoadUnmatchedLeftRow(DataTable result, DataRow leftRow)
    {
        var leftValues = leftRow.ItemArray;
        var joinedValues = new object[result.Columns.Count];
        Array.Copy(leftValues, 0, joinedValues, 0, leftValues.Length);
        result.LoadDataRow(joinedValues, true);
    }

    private static void LoadUnmatchedRightRows(DataTable result, DataTable right, DataRelation relation)
    {
        foreach (DataRow rightRow in right.Rows)
        {
            if (rightRow.GetChildRows(relation).Length != 0)
            {
                continue;
            }

            var rightValues = rightRow.ItemArray;
            var joinedValues = new object[result.Columns.Count];
            Array.Copy(rightValues, 0, joinedValues, joinedValues.Length - rightValues.Length, rightValues.Length);
            result.LoadDataRow(joinedValues, true);
        }
    }

    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a <see cref="List{T}"/> using a caller-provided mapper.
    /// This overload avoids reflection and is suitable for AOT/trimming scenarios.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dataTable">The current <see cref="DataTable"/>.</param>
    /// <param name="map">A mapper that converts each <see cref="DataRow"/> to <typeparamref name="T"/>.</param>
    /// <returns>A <see cref="List{T}"/> representing the rows.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// List&lt;MyClass&gt; list = table.ToList(row =&gt; new MyClass
    /// {
    ///     Id = row["Id"].ToIntOrDefault(),
    ///     Name = row["Name"]?.ToString()
    /// });
    /// </code>
    /// </example>
    public static List<T>? ToList<T>(this DataTable? dataTable, Func<DataRow, T> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (dataTable?.Rows.Count == 0)
        {
            return [];
        }

        if (dataTable is null)
        {
            return null;
        }

        var result = new List<T>(dataTable.Rows.Count);
        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(map(row));
        }

        return result;
    }

    /// <summary>
    /// Creates a reusable column setter delegate with built-in type conversion.
    /// This helper reduces duplicated conversion logic when building <c>columnSetters</c> maps.
    /// </summary>
    /// <typeparam name="T">The target object type.</typeparam>
    /// <typeparam name="TValue">The target property value type.</typeparam>
    /// <param name="assign">Strongly typed property assignment action.</param>
    /// <param name="assignDefaultWhenNull">
    /// If <c>true</c> and the converted value is null, assigns <c>default</c> for nullable/reference targets.
    /// For non-nullable value targets, null input is ignored.
    /// </param>
    /// <returns>A reusable <see cref="Action{T1,T2}"/> that accepts raw column values.</returns>
    /// <example>
    /// <code>
    /// var setters = new Dictionary&lt;string, Action&lt;MyDto, object?&gt;&gt;
    /// {
    ///     ["Id"] = DataTableExtensions.CreateColumnSetter&lt;MyDto, int&gt;((x, v) =&gt; x.Id = v),
    ///     ["Name"] = DataTableExtensions.CreateColumnSetter&lt;MyDto, string?&gt;((x, v) =&gt; x.Name = v),
    ///     ["Age"] = DataTableExtensions.CreateColumnSetter&lt;MyDto, int?&gt;((x, v) =&gt; x.Age = v)
    /// };
    /// </code>
    /// </example>
    public static Action<T, object?> CreateColumnSetter<T, TValue>(
        Action<T, TValue> assign,
        bool assignDefaultWhenNull = true)
    {
        ArgumentNullException.ThrowIfNull(assign);

        return (target, rawValue) =>
        {
            if (rawValue is null or DBNull)
            {
                if (assignDefaultWhenNull && IsNullableOrReferenceType(typeof(TValue)))
                {
                    assign(target, default!);
                }

                return;
            }

            if (!Helper.TypeConverter.TryConvert(rawValue, typeof(TValue), out var convertedValue))
            {
                return;
            }

            if (convertedValue is null)
            {
                if (assignDefaultWhenNull && IsNullableOrReferenceType(typeof(TValue)))
                {
                    assign(target, default!);
                }

                return;
            }

            assign(target, (TValue)convertedValue);
        };
    }

    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a <see cref="List{T}"/> using
    /// a caller-provided factory and column setter map.
    /// This overload avoids reflection and is suitable for AOT/trimming scenarios.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dataTable">The current <see cref="DataTable"/>.</param>
    /// <param name="factory">Factory used to create each target item.</param>
    /// <param name="columnSetters">Column name to setter delegate map. Column matching is case-insensitive.</param>
    /// <returns>A <see cref="List{T}"/> representing the rows.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// List&lt;MyClass&gt; list = table.ToList(
    ///     () =&gt; new MyClass(),
    ///     new Dictionary&lt;string, Action&lt;MyClass, object?&gt;&gt;
    ///     {
    ///         ["Id"] = (x, v) =&gt; x.Id = Convert.ToInt32(v),
    ///         ["Name"] = (x, v) =&gt; x.Name = v?.ToString()
    ///     });
    /// </code>
    /// </example>
    public static List<T>? ToList<T>(
        this DataTable? dataTable,
        Func<T> factory,
        IReadOnlyDictionary<string, Action<T, object?>> columnSetters)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(columnSetters);

        if (dataTable?.Rows.Count == 0)
        {
            return [];
        }

        if (dataTable is null)
        {
            return null;
        }

        if (columnSetters.Count == 0)
        {
            throw new ArgumentException("At least one column setter is required.", nameof(columnSetters));
        }

        var setterLookup = new Dictionary<string, Action<T, object?>>(columnSetters.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in columnSetters)
        {
            setterLookup[pair.Key] = pair.Value;
        }

        var columnMappings = new Dictionary<int, Action<T, object?>>();
        for (var i = 0; i < dataTable.Columns.Count; i++)
        {
            var columnName = dataTable.Columns[i].ColumnName;
            if (setterLookup.TryGetValue(columnName, out var setter))
            {
                columnMappings[i] = setter;
            }
        }

        if (columnMappings.Count == 0)
        {
            throw new ArgumentException("No matching DataTable columns were found for the provided column setters.", nameof(columnSetters));
        }

        var result = new List<T>(dataTable.Rows.Count);
        foreach (DataRow row in dataTable.Rows)
        {
            var item = factory();
            foreach (var mapping in columnMappings)
            {
                var rawValue = row[mapping.Key];
                mapping.Value(item, rawValue == DBNull.Value ? null : rawValue);
            }

            result.Add(item);
        }

        return result;
    }

    private static bool IsNullableOrReferenceType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

}
