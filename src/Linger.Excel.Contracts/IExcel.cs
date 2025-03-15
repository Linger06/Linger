using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Linger.Excel.Contracts;

/// <summary>
/// Excel泛型接口 - 提供特定工作表类型的高级操作
/// </summary>
/// <typeparam name="TWorksheet">工作表类型</typeparam>
public interface IExcel<TWorksheet> where TWorksheet : class
{
    #region Import
    
    /// <summary>
    /// 将Excel文件转换为DataTable
    /// </summary>
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    
    /// <summary>
    /// 将Excel文件转换为对象列表
    /// </summary>
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    
    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    
    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    
    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    
    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    
    #endregion
    
    #region Export
    
    /// <summary>
    /// 数据表格转 Excel 文件，支持自定义操作
    /// </summary>
    string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1", string title = "", 
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);
    
    /// <summary>
    /// 列表转 Excel 文件，支持自定义操作
    /// </summary>
    string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "sheet1", string title = "", 
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;
    
    /// <summary>
    /// 列表转 Excel 内存流，支持自定义操作
    /// </summary>
    MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "", 
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;
    
    /// <summary>
    /// 数据表格转 Excel 内存流，支持自定义操作
    /// </summary>
    MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "", 
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);
    
    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "", 
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);
    
    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "", 
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;
    
    /// <summary>
    /// 创建Excel模板
    /// </summary>
    MemoryStream CreateExcelTemplate<T>() where T : class, new();
    
    #endregion
}