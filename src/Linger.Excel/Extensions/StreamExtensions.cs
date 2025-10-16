using System.Data;

namespace Linger.Excel.Extensions;

/// <summary>
///     <see cref="Stream" /> 扩展
/// </summary>
public static class StreamExtensions
{
    #region 将当前 Stream 对象读取到 DataTable

    /// <summary>
    ///     将当前 <see cref="Stream" /> 对象读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="sheetName">指定工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性为 小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public static DataTable? ReadToDataTable(this Stream stream, string? sheetName = null,
        bool firstRowIsColumnName = true, bool addEmptyRow = false)
    {
        return new Excel().ReadStreamToDataTable(stream, sheetName, firstRowIsColumnName, addEmptyRow);
    }

    #endregion 将当前 Stream 对象读取到 DataTable

    #region 将当前 Stream 用异步方式读取到 DataTable

    /// <summary>
    ///     当前 <see cref="Stream" /> 用异步方式读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性为 小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public static async Task<DataTable?> ReadToDataTableAsync(this Stream stream, string? sheetName = null,
        bool firstRowIsColumnName = true, bool addEmptyRow = false)
    {
        return await new Excel().ReadStreamToDataTableAsync(stream, sheetName, firstRowIsColumnName, addEmptyRow)
            .ConfigureAwait(false);
    }

    #endregion 将当前 Stream 用异步方式读取到 DataTable

    #region 将当前 Stream 对象读取到 ICollection<DataTable>

    /// <summary>
    ///     将当前 <see cref="Stream" /> 对象读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public static ICollection<DataTable>? ReadToTables(this Stream stream, bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        return new Excel().ReadStreamToTables(stream, firstRowIsColumnName, addEmptyRow);
    }

    #endregion 将当前 Stream 对象读取到 ICollection<DataTable>

    #region 将当前 Stream 用异步方式读取到 ICollection<DataTable>

    /// <summary>
    ///     当前 <see cref="Stream" /> 用异步方式读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public static async Task<ICollection<DataTable>?> ReadToTablesAsync(this Stream stream,
        bool firstRowIsColumnName = true, bool addEmptyRow = false)
    {
        return await new Excel().ReadStreamToTablesAsync(stream, firstRowIsColumnName, addEmptyRow)
            .ConfigureAwait(false);
    }

    #endregion 将当前 Stream 用异步方式读取到 ICollection<DataTable>
}