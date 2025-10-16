using System.Data;
using OfficeOpenXml;

namespace Linger.Excel.Extensions;

/// <summary>
///     <see cref="DataSet" /> 扩展
/// </summary>
public static class DataSetExtensions
{
    #region 将当前 DataSet 对象写入 File

    /// <summary>
    ///     将当前 <see cref="DataSet" /> 写入 <see cref="System.IO.File" />
    /// </summary>
    /// <param name="dataSet">要写入的 <see cref="DataSet" /> 对象</param>
    /// <param name="fullFileName"></param>
    /// <param name="actions">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 对象</returns>
    public static string ExportToFile(this DataSet dataSet, string fullFileName,
        List<Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>>? actions = null,
        string? sheetsName = null)
    {
        return new Excel().ExportToFile(dataSet, fullFileName, actions, sheetsName);
    }

    #endregion 将当前 DataSet 对象写入 File

    #region 将当前 DataSet 对象写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="DataSet" /> 写入 <see cref="System.IO.File" />
    /// </summary>
    /// <param name="dataSet">要写入的 <see cref="DataSet" /> 对象</param>
    /// <param name="actions">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 对象</returns>
    public static MemoryStream? ExportToMemoryStream(this DataSet dataSet, List<Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>>? actions = null,
        string? sheetsName = null)
    {
        return new Excel().ExportToMemoryStream(dataSet, actions, sheetsName);
    }
    #endregion 
}