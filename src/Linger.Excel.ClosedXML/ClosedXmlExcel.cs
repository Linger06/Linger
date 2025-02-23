using System.Data;
using ClosedXML.Excel;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
namespace Linger.Excel.ClosedXML;

public class ClosedXmlExcel : ExcelBase
{
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        if (list.IsNull())
        {
            return null; 
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetsName);
        
        // Add title if specified
        var currentRow = 1;
        if(!string.IsNullOrEmpty(title))
        {
            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Range(1, 1, 1, list.GetType().GetProperties().Length).Merge();
            currentRow++;
        }
        
        // Add headers
        var properties = typeof(T).GetProperties();
        for (var i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
        }

        // Add data
        for (var i = 0; i < list.Count; i++)
        {
            for (var j = 0; j < properties.Length; j++)
            {
                var value = properties[j].GetValue(list[i]);
                worksheet.Cell(i + currentRow + 1, j + 1).Value = value?.ToString() ?? string.Empty;
            }
        }

        // 执行自定义操作
        action?.Invoke(worksheet, properties);

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "",Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable.IsNull())
        {
            return null;
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetsName);

        // Add title if specified
        var currentRow = 1;
        if (!string.IsNullOrEmpty(title))
        {
            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Range(1, 1, 1, dataTable.Columns.Count).Merge();
            currentRow++;
        }
        
        // Add headers
        for (var i = 0; i < dataTable.Columns.Count; i++)
        {
            worksheet.Cell(currentRow, i + 1).Value = dataTable.Columns[i].ColumnName;
        }

        // Add data
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            for (var j = 0; j < dataTable.Columns.Count; j++)
            {
                worksheet.Cell(i + currentRow + 1, j + 1).Value = dataTable.Rows[i][j].ToString();
            }
        }

        // 执行自定义操作
        action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        using var workbook = new XLWorkbook(stream);
        IXLWorksheet worksheet;
        
        if (string.IsNullOrEmpty(sheetName))
            worksheet = workbook.Worksheet(1);
        else
            worksheet = workbook.Worksheet(sheetName);

        var dt = new DataTable();
        var firstRow = true;
        var startRow = columnNameRowIndex + 1;

        foreach (IXLRow row in worksheet.RowsUsed())
        {
            if (row.RowNumber() < startRow) continue;
            
            if (firstRow)
            {
                foreach (IXLCell cell in row.Cells())
                {
                    dt.Columns.Add(cell.Value.ToString());
                }
                firstRow = false;
            }
            else
            {
                var toInsert = dt.NewRow();
                var i = 0;
                foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                {
                    toInsert[i++] = cell.Value;
                }
                dt.Rows.Add(toInsert);
            }
        }

        if (dispose)
        {
            stream.Dispose();
        }

        return dt;
    }

    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true)
    {
        var dt = ConvertStreamToDataTable(stream, sheetName, columnNameRowIndex, addEmptyRow, dispose);
        if (dt == null) return null;

        var list = new List<T>();
        var properties = typeof(T).GetProperties();

        foreach (DataRow row in dt.Rows)
        {
            var item = new T();
            foreach (var prop in properties)
            {
                if (!dt.Columns.Contains(prop.Name)) continue;
                
                var value = row[prop.Name];
                if (value != DBNull.Value)
                {
                    prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                }
            }
            list.Add(item);
        }

        return list;
    }
}
