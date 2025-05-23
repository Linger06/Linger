# Linger.Excel.ClosedXML

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.Excel.ClosedXML is an implementation of the Excel manipulation interfaces defined in Linger.Excel.Contracts, based on the powerful [ClosedXML](https://github.com/ClosedXML/ClosedXML) library. This package provides high-performance Excel file operations with a clean, abstracted API.

## Features

- Read and write Excel files (.xlsx format) with a simple, intuitive API
- Convert Excel data to strongly-typed object collections and vice versa 
- Data table import and export capabilities
- Template-based Excel generation
- Advanced styling and formatting options
- Cell formula support
- Multi-worksheet management
- Comprehensive error handling

## Installation

```shell
dotnet add package Linger.Excel.ClosedXML
```

## Usage

### Basic Setup

```csharp
// Register in DI container
services.AddExcelClosedXML(options => 
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.AutoFitColumns = true;
});

// Or create an instance directly
var excel = new ClosedXmlExcel(new ExcelOptions 
{
    DefaultDateFormat = "yyyy-MM-dd",
    AutoFitColumns = true  
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
```

## Dependencies

- ClosedXML (v0.105.0+)
- Linger.Excel.Contracts

## Related Packages

- [Linger.Excel.Contracts](../Linger.Excel.Contracts/) - Core interfaces and abstractions
- [Linger.Excel.Npoi](../Linger.Excel.Npoi/) - Alternative implementation using NPOI library

## Target Frameworks

- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
