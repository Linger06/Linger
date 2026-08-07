namespace Linger.Excel.Contracts;

/// <summary>
/// 表示从 Excel 工作表读取的一行数据。
/// </summary>
/// <remarks>
/// 行对象通过共享的列名索引访问当前行值，不依赖 <see cref="DataTable"/> 或 <see cref="DataRow"/>。
/// </remarks>
public readonly struct ExcelRow
{
    private readonly IReadOnlyDictionary<string, int>? _columnIndexes;
    private readonly object[]? _values;

    internal ExcelRow(IReadOnlyDictionary<string, int> columnIndexes, object[] values)
    {
        _columnIndexes = columnIndexes;
        _values = values;
    }

    /// <summary>
    /// 获取当前行的列数。
    /// </summary>
    public int ColumnCount => _values?.Length ?? 0;

    /// <summary>
    /// 按列索引获取值；空单元格返回 <see langword="null"/>。
    /// </summary>
    /// <param name="columnIndex">从零开始的列索引。</param>
    /// <returns>单元格值。</returns>
    /// <exception cref="InvalidOperationException">当前实例未由 Excel 导入流程初始化。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="columnIndex"/> 超出有效范围。</exception>
    public object? this[int columnIndex]
    {
        get
        {
            var values = GetValues();
            if ((uint)columnIndex >= (uint)values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            return NormalizeValue(values[columnIndex]);
        }
    }

    /// <summary>
    /// 按列名获取值；列名匹配不区分大小写，空单元格返回 <see langword="null"/>。
    /// </summary>
    /// <param name="columnName">列名。</param>
    /// <returns>单元格值。</returns>
    /// <exception cref="InvalidOperationException">当前实例未由 Excel 导入流程初始化。</exception>
    /// <exception cref="KeyNotFoundException">找不到指定列。</exception>
    public object? this[string columnName] => this[GetOrdinal(columnName)];

    /// <summary>
    /// 判断当前行是否包含指定列。
    /// </summary>
    /// <param name="columnName">列名。</param>
    /// <returns>包含指定列时返回 <see langword="true"/>。</returns>
    public bool ContainsColumn(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        return _columnIndexes?.ContainsKey(columnName) == true;
    }

    /// <summary>
    /// 获取指定列名对应的从零开始索引。
    /// </summary>
    /// <param name="columnName">列名。</param>
    /// <returns>列索引。</returns>
    /// <exception cref="InvalidOperationException">当前实例未由 Excel 导入流程初始化。</exception>
    /// <exception cref="KeyNotFoundException">找不到指定列。</exception>
    public int GetOrdinal(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var columnIndexes = _columnIndexes ?? throw new InvalidOperationException("ExcelRow 尚未初始化。");
        if (!columnIndexes.TryGetValue(columnName, out var columnIndex))
        {
            throw new KeyNotFoundException($"找不到 Excel 列: '{columnName}'。");
        }

        return columnIndex;
    }

    /// <summary>
    /// 判断指定列是否为空。
    /// </summary>
    /// <param name="columnName">列名。</param>
    /// <returns>单元格为空时返回 <see langword="true"/>。</returns>
    public bool IsNull(string columnName)
    {
        return this[columnName] is null;
    }

    /// <summary>
    /// 获取指定列并转换为目标类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="columnName">列名。</param>
    /// <returns>转换后的值；空单元格返回 <see langword="default"/>。</returns>
    /// <exception cref="InvalidCastException">单元格值无法转换为目标类型。</exception>
    public T? Get<T>(string columnName)
    {
        if (TryGet<T>(columnName, out var value))
        {
            return value;
        }

        var rawValue = this[columnName];
        throw new InvalidCastException(
            $"Excel 列 '{columnName}' 的值 '{rawValue}' 无法转换为 {typeof(T).FullName}。");
    }

    /// <summary>
    /// 尝试获取指定列并转换为目标类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="columnName">列名。</param>
    /// <param name="value">转换成功后的值。</param>
    /// <returns>转换成功时返回 <see langword="true"/>。</returns>
    public bool TryGet<T>(string columnName, out T? value)
    {
        var rawValue = this[columnName];
        if (rawValue is null)
        {
            value = default;
            return true;
        }

        if (rawValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        if (TypeConverter.TryConvert(rawValue, typeof(T), out var convertedValue) && convertedValue is T converted)
        {
            value = converted;
            return true;
        }

        value = default;
        return false;
    }

    private object[] GetValues()
    {
        return _values ?? throw new InvalidOperationException("ExcelRow 尚未初始化。");
    }

    private static object? NormalizeValue(object? value)
    {
        return value is DBNull ? null : value;
    }
}
