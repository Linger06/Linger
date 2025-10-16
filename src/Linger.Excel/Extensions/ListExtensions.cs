using System.Reflection;
using OfficeOpenXml;

namespace Linger.Excel.Extensions;

public static class ListExtensions
{
    #region 将当前 List 集合写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="List{T}" /> 写入 <see cref="MemoryStream" />
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public static MemoryStream? WriteToMemoryStream<T>(this List<T> list, string sheetsName = "sheet1",
        string title = "")
    {
        return new Excel().WriteToMemoryStream(list, null, sheetsName, title);
    }

    #endregion 将当前 List 集合写入 MemoryStream

    #region 将当前 List 集合写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="List{T}" /> 写入 <see cref="MemoryStream" />
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="action">用于执行写入 Excel 单元格的函数</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public static MemoryStream? WriteToMemoryStream<T>(this List<T> list,
        Action<ExcelWorksheet, PropertyInfo[]> action,
        string sheetsName = "sheet1")
    {
        return new Excel().WriteToMemoryStream(list, action, sheetsName);
    }

    #endregion 将当前 List 集合写入 MemoryStream

    #region 将当前 List 集合用异步方式写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="List{T}" /> 集合用异步方式写入 <see cref="MemoryStream" />
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public static async Task<MemoryStream?> WriteToMemoryStreamAsync<T>(this List<T> list,
        string sheetsName = "sheet1",
        string title = "")
    {
        return await new Excel().WriteToMemoryStreamAsync(list, null, sheetsName, title).ConfigureAwait(false);
    }

    #endregion 将当前 List 集合用异步方式写入 MemoryStream

    #region 将当前 List 集合用异步方式写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="List{T}" /> 集合用异步方式写入 <see cref="MemoryStream" />
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="action">用于执行写入 Excel 单元格的函数</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public static async Task<MemoryStream?> WriteToMemoryStreamAsync<T>(this List<T> list,
        Action<ExcelWorksheet, PropertyInfo[]> action, string sheetsName = "sheet1")
    {
        return await new Excel().WriteToMemoryStreamAsync(list, action, sheetsName).ConfigureAwait(false);
    }

    #endregion 将当前 List 集合用异步方式写入 MemoryStream
}