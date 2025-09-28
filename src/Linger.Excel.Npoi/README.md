# Linger.Excel.Npoi

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.Excel.Npoi is an implementation of the Excel manipulation interfaces defined in Linger.Excel.Contracts, based on the powerful [NPOI](https://github.com/nissl-lab/npoi) library. This package provides high-performance Excel file operations without requiring Microsoft Office to be installed.

## Features

- Read and write Excel files (.xlsx and .xls formats) with a simple, intuitive API
- Convert Excel data to strongly-typed object collections and vice versa
- Data table import and export capabilities
- Template-based Excel generation
- Advanced styling and formatting options
- Cell formula support and evaluation
- Multi-worksheet management
- Comprehensive error handling
- Support for legacy Excel formats (.xls)

## Installation

```shell
dotnet add package Linger.Excel.Npoi
```

## Usage

### Basic Setup

```csharp
// Register in DI container
services.AddExcelNpoi(options => 
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.EnableFormulaEvaluation = true;
});

// Or create an instance directly
var excel = new NpoiExcel(new ExcelOptions 
{
    DefaultDateFormat = "yyyy-MM-dd",
    EnableFormulaEvaluation = true
});
```

### Reading Excel Files

```csharp
// Convert Excel to List<T>
var users = excel.ExcelToList<UserModel>("users.xlsx");

// Convert Excel to DataTable
var dataTable = excel.ExcelToDataTable("data.xlsx", sheetName: "Sheet1");

// Read from stream
using var stream = File.OpenRead("data.xlsx");
var results = excel.ExcelToList<ResultModel>(stream);
```

### Writing Excel Files

```csharp
// Convert List<T> to Excel file
var users = GetUsers(); // Your data source
excel.ListToExcel(users, "users.xlsx");

// Convert DataTable to Excel file
var dataTable = GetDataTable(); // Your data source
excel.DataTableToExcel(dataTable, "data.xlsx");

// Export to stream (for web downloads, etc.)
using var memoryStream = new MemoryStream();
excel.ListToExcel(products, memoryStream);
return File(memoryStream.ToArray(), 
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
    "products.xlsx");
```

### Advanced Features

```csharp
// Multi-sheet operations
var workbook = excel.CreateWorkbook();
excel.AddSheet(workbook, "Users", users);
excel.AddSheet(workbook, "Products", products);
excel.SaveWorkbook(workbook, "company_data.xlsx");

// Styling
excel.ListToExcel(salesData, "sales_report.xlsx", styleOptions => 
{
    styleOptions.HeaderBackgroundColor = "#1E90FF";
    styleOptions.HeaderFontColor = "#FFFFFF";
    styleOptions.AlternatingRowBackgroundColor = "#F0F8FF";
});

// Formula evaluation
var options = new ExcelOptions
{
    EnableFormulaEvaluation = true
};
var excelWithFormulas = new NpoiExcel(options);
var calculatedData = excelWithFormulas.ExcelToList<CalculationModel>("formulas.xlsx");
```

## NPOI-specific Features

### Support for Legacy Excel Formats

```csharp
// Reading .xls files
var legacyData = excel.ExcelToList<LegacyData>("old_data.xls");

// Writing .xls files
excel.ListToExcel(legacyData, "legacy_export.xls");
```

### Cell Comments

```csharp
// Access to NPOI-specific features
IWorkbook workbook = (IWorkbook)excel.CreateWorkbook();
ISheet sheet = workbook.CreateSheet("Comments");
IRow row = sheet.CreateRow(0);
ICell cell = row.CreateCell(0);
cell.SetCellValue("Cell with comment");

// Add a comment
IDrawing drawing = sheet.CreateDrawingPatriarch();
IComment comment = drawing.CreateCellComment(new XSSFClientAnchor(0, 0, 0, 0, 0, 0, 5, 5));
comment.String = new XSSFRichTextString("This is a comment");
comment.Author = "System";
cell.CellComment = comment;

excel.SaveWorkbook(workbook, "with_comments.xlsx");
```

## Dependencies

- NPOI (v2.7.3+)
- Linger.Excel.Contracts

## Related Packages

- [Linger.Excel.Contracts](../Linger.Excel.Contracts/) - Core interfaces and abstractions
- [Linger.Excel.ClosedXML](../Linger.Excel.ClosedXML/) - Alternative implementation using ClosedXML library

## Target Frameworks

- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
