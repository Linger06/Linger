using Linger.Extensions.Core;

namespace Linger.Extensions.Data;

/// <summary>
/// Extensions for <see cref="DataTable"/>.
/// </summary>
public static partial class DataTableExtensions
{
#if NET451_OR_GREATER || NETSTANDARD|| NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously converts the current <see cref="DataTable"/> to a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dt">The <see cref="DataTable"/> to convert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the converted <see cref="List{T}"/>.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// List&lt;MyClass&gt; list = await table.ToListAsync&lt;MyClass&gt;();
    /// </code>
    /// </example>
    public static Task<List<T>?> ToListAsync<T>(this DataTable dt) where T : class, new()
    {
        return Task.FromResult(dt.ToList<T>(1000));
        //return await Task.Run(dt.ToList<T>).ConfigureAwait(false);
    }
#endif

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
        double sum = 0;

        foreach (DataRow dr in sourceTable.Rows)
        {
            sum += Convert.ToDouble(dr[columnName]);
        }

        return sum;
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
    /// <exception cref="System.Exception">Thrown when the specified columns are not found in the respective tables.</exception>
    public static DataTable Join(this DataTable left, DataTable right, DataColumn[] leftCols, DataColumn[] rightCols,
        bool includeLeftJoin, bool includeRightJoin)
    {
        leftCols.ForEach(x =>
        {
            if (!left.ContainAllColumns(x.ColumnName))
            {
                throw new System.Exception($"{nameof(leftCols)} have columns not in {nameof(left)}");
            }
        });

        rightCols.ForEach(x =>
        {
            if (!right.ContainAllColumns(x.ColumnName))
            {
                throw new System.Exception($"{nameof(rightCols)} have columns not in {nameof(right)}");
            }
        });

        DataTable result = new("JoinResult");
        using DataSet ds = new();
        ds.Tables.AddRange([left.Copy(), right.Copy()]);
        var leftRelationCols = new DataColumn[leftCols.Length];
        for (var i = 0; i < leftCols.Length; i++)
        {
            leftRelationCols[i] = ds.Tables[0].Columns[leftCols[i].ColumnName]!;
        }

        var rightRelationCols = new DataColumn[rightCols.Length];
        for (var i = 0; i < rightCols.Length; i++)
        {
            rightRelationCols[i] = ds.Tables[1].Columns[rightCols[i].ColumnName]!;
        }

        //create result columns
        for (var i = 0; i < left.Columns.Count; i++)
        {
            _ = result.Columns.Add(left.Columns[i].ColumnName, left.Columns[i].DataType);
        }

        for (var i = 0; i < right.Columns.Count; i++)
        {
            var colName = right.Columns[i].ColumnName;
            while (result.Columns.Contains(colName))
            {
                colName += "_2";
            }

            _ = result.Columns.Add(colName, right.Columns[i].DataType);
        }

        //add left join relations
        DataRelation drLeftJoin = new("rLeft", leftRelationCols, rightRelationCols, false);
        ds.Relations.Add(drLeftJoin);

        //join
        result.BeginLoadData();
        foreach (DataRow parentRow in ds.Tables[0].Rows)
        {
            DataRow[] childrenRowList = parentRow.GetChildRows(drLeftJoin);
            if (childrenRowList.Length > 0)
            {
                var parentArray = parentRow.ItemArray;
                foreach (DataRow childRow in childrenRowList)
                {
                    var childArray = childRow.ItemArray;
                    var joinArray = new object[parentArray.Length + childArray.Length];
                    Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                    Array.Copy(childArray, 0, joinArray, parentArray.Length, childArray.Length);
                    _ = result.LoadDataRow(joinArray, true);
                }
            }
            else //left join
            {
                if (includeLeftJoin)
                {
                    var parentArray = parentRow.ItemArray;
                    var joinArray = new object[parentArray.Length];
                    Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                    _ = result.LoadDataRow(joinArray, true);
                }
            }
        }

        if (includeRightJoin)
        {
            //add right join relations
            DataRelation drRightJoin = new("rRight", rightRelationCols, leftRelationCols, false);
            ds.Relations.Add(drRightJoin);

            foreach (DataRow parentRow in ds.Tables[1].Rows)
            {
                DataRow[] childrenRowList = parentRow.GetChildRows(drRightJoin);
                if (childrenRowList.Length == 0)
                {
                    var parentArray = parentRow.ItemArray;
                    var joinArray = new object[result.Columns.Count];
                    Array.Copy(parentArray, 0, joinArray,
                        joinArray.Length - parentArray.Length, parentArray.Length);
                    _ = result.LoadDataRow(joinArray, true);
                }
            }
        }

        result.EndLoadData();

        return result;
    }
}
