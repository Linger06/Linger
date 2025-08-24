using System.Collections.Concurrent;
using System.Reflection;
using Linger.Extensions.Core;
using Linger.JsonConverter;

namespace Linger.Extensions.Data;

/// <summary>
/// Extensions for <see cref="DataTable"/> with performance optimizations.
/// </summary>
public static class DataTableExtensions
{
    // Performance optimization: Cache property mappings to avoid repeated reflection calls
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> s_propertyMappingCache = new();

    // Cache for type property arrays to reduce reflection overhead
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_typePropertiesCache = new();

#if NET451_OR_GREATER || NETSTANDARD|| NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously converts the current <see cref="DataTable"/> to a <see cref="List{T}"/> with performance optimizations.
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
        return Task.FromResult(dt.ToList<T>());
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
            sum += dr[columnName].ToDoubleOrDefault();
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
    /// <exception cref="Exception">Thrown when the specified columns are not found in the respective tables.</exception>
    public static DataTable Join(this DataTable left, DataTable right, DataColumn[] leftCols, DataColumn[] rightCols,
        bool includeLeftJoin, bool includeRightJoin)
    {
        leftCols.ForEach(x =>
        {
            if (!left.ContainAllColumns(x.ColumnName))
            {
                throw new System.ArgumentException($"{nameof(leftCols)} have columns not in {nameof(left)}");
            }
        });

        rightCols.ForEach(x =>
        {
            if (!right.ContainAllColumns(x.ColumnName))
            {
                throw new System.ArgumentException($"{nameof(rightCols)} have columns not in {nameof(right)}");
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

        // 验证所有列都属于源表
        foreach (var column in groupColumns.Concat(captionColumns).Append(valueColumn))
        {
            if (!source.Columns.Contains(column.ColumnName))
            {
                throw new System.ArgumentException($"列 '{column.ColumnName}' 不存在于源数据表中");
            }
        }

        // 创建结果表
        var resultTable = new DataTable();

        // 添加分组列作为结果表的列
        foreach (DataColumn groupColumn in groupColumns)
        {
            resultTable.Columns.Add(groupColumn.ColumnName, groupColumn.DataType);
        }

        // 如果源表没有数据，直接返回只包含列定义的空表
        if (source.Rows.Count == 0)
        {
            return resultTable;
        }

        // 获取不同的列标题
        var distinctCaptionValues = new HashSet<string>();
        foreach (DataRow row in source.Rows)
        {
            var captionValue = FormatCaptionValue(row, captionColumns);
            distinctCaptionValues.Add(captionValue);
        }

        // 添加列标题作为新列
        foreach (var captionValue in distinctCaptionValues)
        {
            if (!resultTable.Columns.Contains(captionValue) &&
                !string.IsNullOrEmpty(captionValue))
            {
                var newColumn = resultTable.Columns.Add(captionValue, typeof(decimal));
                newColumn.AllowDBNull = true;
            }
        }

        // 使用字典存储分组键与其对应的数据行
        var groupedRows = new Dictionary<string, DataRow>();

        // 分组处理数据
        foreach (DataRow sourceRow in source.Rows)
        {
            // 使用分隔符生成分组键，避免歧义
            var groupKey = GenerateGroupKey(sourceRow, groupColumns);

            // 获取或创建分组对应的结果行
            if (!groupedRows.TryGetValue(groupKey, out DataRow? resultRow))
            {
                resultRow = resultTable.NewRow();

                // 设置分组列的值
                foreach (DataColumn groupColumn in groupColumns)
                {
                    resultRow[groupColumn.ColumnName] = sourceRow[groupColumn.ColumnName];
                }

                resultTable.Rows.Add(resultRow);
                groupedRows[groupKey] = resultRow;
            }

            // 获取列标题和值
            var captionValue = FormatCaptionValue(sourceRow, captionColumns);

            if (resultTable.Columns.Contains(captionValue))
            {
                try
                {
                    // 获取值并设置到对应单元格
                    var cellValue = sourceRow[valueColumn];
                    if (cellValue != DBNull.Value)
                    {
                        decimal currentValue = 0;

                        // 如果单元格已有值，则获取现有值
                        if (resultRow[captionValue] != DBNull.Value)
                        {
                            var value = resultRow[captionValue];
                            currentValue = value.ToDecimalOrDefault();
                        }

                        // 将新值添加到现有值
                        decimal newValue = cellValue.ToDecimalOrDefault();

                        resultRow[captionValue] = currentValue + newValue;
                    }
                }
                catch (Exception ex)
                {
                    // 记录异常但继续处理其他行
                    Console.WriteLine($"处理值时出错: {ex.Message}");
                }
            }
        }

        return resultTable;
    }

    // 生成唯一的分组键，使用特殊分隔符避免歧义
    private static string GenerateGroupKey(DataRow row, DataColumn[] groupColumns)
    {
        var keyParts = new List<string>();
        foreach (DataColumn column in groupColumns)
        {
            var value = row[column];
            // 确保null和DBNull值也能生成唯一键
            var stringValue = value == DBNull.Value ? "<NULL>" : value?.ToString() ?? "<NULL>";
            keyParts.Add(stringValue);
        }

        // 使用不太可能出现在数据中的分隔符
        return string.Join("||:||", keyParts);
    }

    // 格式化列标题值
    private static string FormatCaptionValue(DataRow row, DataColumn[] captionColumns)
    {
        var captionParts = new List<string>();
        foreach (DataColumn column in captionColumns)
        {
            var value = row[column];
            // 确保null和DBNull值也能生成有效的列名
            var stringValue = value == DBNull.Value ? "NULL" : value?.ToString() ?? "NULL";

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
        foreach (DataRow item in rows)
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
    /// Converts the current <see cref="DataTable"/> to a <see cref="List{T}"/> with advanced performance optimizations.
    /// Uses caching to minimize reflection overhead and supports parallel processing for large datasets.
    /// </summary>
    /// <typeparam name="T">The type of elements to convert to.</typeparam>
    /// <param name="dataTable">The current <see cref="DataTable"/>.</param>
    /// <param name="parallelProcessingThreshold">The minimum number of rows to enable parallel processing (default: 1000).</param>
    /// <returns>A <see cref="List{T}"/> representing the rows.</returns>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// List&lt;MyClass&gt; list = table.ToList&lt;MyClass&gt;(500); // Enable parallel processing for 500+ rows
    /// </code>
    /// </example>
    public static List<T>? ToList<T>(this DataTable? dataTable, int parallelProcessingThreshold = 1000) where T : class, new()
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

        // Get cached property mapping for the type
        var propertyMap = GetCachedPropertyMapping<T>();

        // Build column to property mappings
        var columnMappings = new Dictionary<int, PropertyInfo>();
        for (var i = 0; i < dataTable.Columns.Count; i++)
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
        var useParallel = dataTable.Rows.Count > parallelProcessingThreshold;

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
                    var colIndex = mapping.Key;
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
                    var colIndex = mapping.Key;
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
    }    /// <summary>
         /// Gets cached property mapping for the specified type to minimize reflection overhead.
         /// Only includes properties that have public setters.
         /// </summary>
         /// <typeparam name="T">The type to get property mapping for.</typeparam>
         /// <returns>A dictionary mapping property names to PropertyInfo objects.</returns>
    private static Dictionary<string, PropertyInfo> GetCachedPropertyMapping<T>() where T : class
    {
        return s_propertyMappingCache.GetOrAdd(typeof(T), type =>
        {
            var properties = GetCachedTypeProperties(type);
            // Only include properties with public setters
            var writableProperties = properties.Where(p => p.CanWrite && p.SetMethod?.IsPublic == true);
            return writableProperties.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Gets cached type properties to minimize reflection overhead.
    /// </summary>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>An array of PropertyInfo objects.</returns>
    private static PropertyInfo[] GetCachedTypeProperties(Type type)
    {
        return s_typePropertiesCache.GetOrAdd(type, t => t.GetProperties());
    }

    /// <summary>
    /// Safely sets a property value with type conversion and error handling.
    /// </summary>
    /// <typeparam name="T">The target object type.</typeparam>
    /// <param name="obj">The target object.</param>
    /// <param name="property">The property to set.</param>
    /// <param name="value">The value to set.</param>
    private static void SetPropertySafely<T>(T obj, PropertyInfo property, object? value) where T : class
    {
        if (!property.CanWrite || value == DBNull.Value)
            return;

        try
        {
            // Performance optimization: Use type-specific conversion based on property type
            var convertedValue = ConvertValueToPropertyType(value, property.PropertyType);
            property.SetValue(obj, convertedValue);
        }
        catch
        {
            // Silent fail for incompatible conversions to maintain compatibility
            // In production, you might want to log this or use a different strategy
        }
    }

    /// <summary>
    /// Optimized value conversion that minimizes boxing and uses efficient conversion paths.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The converted value.</returns>
    private static object? ConvertValueToPropertyType(object? value, Type targetType)
    {
        if (value is null || value == DBNull.Value)
            return null;

        // Fast path for exact type matches (avoids conversion overhead)
        if (value.GetType() == targetType)
            return value;        // Handle nullable types
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;        // Handle enums specifically
        if (actualType.IsEnum)
        {
            if (value == null || value == DBNull.Value)
                return Enum.ToObject(actualType, 0);

            if (value is string stringValue)
            {
                return Enum.Parse(actualType, stringValue, true);
            }

            // For numeric values, convert to the enum
            return Enum.ToObject(actualType, value);
        }

        // Special case: Converting double to DateTime (assume OADate)
        if (actualType == typeof(DateTime) && value is double doubleValue)
        {
            return DateTime.FromOADate(doubleValue);
        }

        // Optimized conversion using pattern matching for common types
        return actualType.Name switch
        {
            nameof(String) => value.ToStringOrDefault(),
            nameof(Int16) => value.ToShortOrDefault(),
            nameof(Int32) => value.ToIntOrDefault(),
            nameof(Int64) => value.ToLongOrDefault(),
            nameof(Single) => value.ToFloatOrDefault(),
            nameof(Double) => value.ToDoubleOrDefault(),
            nameof(Decimal) => value.ToDecimalOrDefault(),
            nameof(Boolean) => value.ToBoolOrDefault(),
            nameof(DateTime) => value.ToDateTimeOrDefault(),
            nameof(Guid) => value.ToGuidOrDefault(),
            _ => Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture) // Fallback for other types
        };
    }
}
