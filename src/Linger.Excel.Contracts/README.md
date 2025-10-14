# Linger Excel Framework

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

<div align="center">

![Linger Excel Framework](https://img.shields.io/badge/Linger-Excel%20Framework-brightgreen)

A unified, efficient, and extensible Excel operation framework that supports multiple Excel library implementations

</div>

## 🚀 Features Overview

- **Unified Interface** - Multiple underlying implementations, developers don't need to worry about specific details
- **Automatic Type Mapping** - Seamless data conversion between Excel and objects
- **DataSet Support** - Import/export entire workbook as DataSet, supports multi-sheet operations
- **Dependency Injection Friendly** - Supports .NET Core/ASP.NET Core dependency injection
- **High-Performance Design** - Batch processing, parallel processing, and performance monitoring
- **Async Support** - Comprehensive asynchronous API support
- **Flexible Configuration** - Rich options configuration system
- **Extensibility** - Easy to customize and extend
- **Cross-Platform Compatible** - Supports .NET Standard 2.0+, .NET Core 3.1+, .NET 5+

## 📦 Supported Excel Implementations

| Implementation | Package Name | Features |
|----------------|--------------|----------|
| **NPOI** | `Linger.Excel.Npoi` | No Office required, supports .xls and .xlsx |
| **EPPlus** | `Linger.Excel.EPPlus` | High performance, supports richer Excel features |
| **ClosedXML** | `Linger.Excel.ClosedXML` | User-friendly API, good performance |

## 🏗️ Architecture Design

```
┌─────────────────┐
│  IExcelService  │ ◄──── Non-generic interface, basic Excel operations
└────────┬────────┘
         │
         │implements
         ▼
┌─────────────────┐
│IExcel<TWorksheet>│ ◄──── Generic interface, advanced Excel operations
└────────┬────────┘
         │
         │implements
         ▼
┌─────────────────────────────┐
│AbstractExcelService<T1,T2>  │ ◄──── Abstract base class, common logic
└────────────┬────────────────┘
             │
             │inherits
             ▼
┌─────────────────────────────┐
│ ExcelBase<TWorkbook,TSheet> │ ◄──── Excel implementation base, more common logic
└────────────┬────────────────┘
             │
             │inherits
             ▼
┌─────────────────────────────┐
│  Concrete Implementation    │ ◄──── Specific Excel library implementation
│   (e.g., NpoiExcel)         │       (NpoiExcel, EPPlusExcel, ClosedXmlExcel)
└─────────────────────────────┘
```

## 🚀 Quick Start

### 1. Install Packages

```bash
# Install core interface package
dotnet add package Linger.Excel.Contracts

# Install your chosen implementation
dotnet add package Linger.Excel.Npoi
# or
dotnet add package Linger.Excel.ClosedXML
```

### 2. Register Services

```csharp
// In Startup.cs or Program.cs
// NPOI implementation
services.AddExcelNpoi(options =>
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.EnableFormulaEvaluation = true;
});

// Or use ClosedXML implementation
services.AddExcelClosedXML(options =>
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.AutoFitColumns = true;
});
```

### 3. Use in Services

```csharp
public class ExcelReportService
{
    private readonly IExcelService _excelService;

    public ExcelReportService(IExcelService excelService)
    {
        _excelService = excelService;
    }

    public List<User> ImportUsers(Stream excelStream)
    {
        return _excelService.ExcelToList<User>(excelStream);
    }

    public byte[] ExportUsers(List<User> users)
    {
        using var ms = new MemoryStream();
        _excelService.ListToExcel(users, ms);
        return ms.ToArray();
    }
}
```

## 📝 Core Interfaces

### IExcel<TWorksheet>

```csharp
public interface IExcel<out TWorksheet> where TWorksheet : class
{
    // Import functions
    DataTable ExcelToDataTable(string filePath, string sheetName = null);
    DataTable ExcelToDataTable(Stream stream, string sheetName = null);
    List<T> ExcelToList<T>(string filePath, string sheetName = null) where T : class, new();
    List<T> ExcelToList<T>(Stream stream, string sheetName = null) where T : class, new();

    // Export functions
    void ListToExcel<T>(List<T> data, string filePath, Action<ExcelStyleOptions> styleAction = null) where T : class;
    void ListToExcel<T>(List<T> data, Stream stream, Action<ExcelStyleOptions> styleAction = null) where T : class;
    void DataTableToExcel(DataTable dataTable, string filePath, Action<ExcelStyleOptions> styleAction = null);
    void DataTableToExcel(DataTable dataTable, Stream stream, Action<ExcelStyleOptions> styleAction = null);

    // Workbook/Worksheet operations
    object CreateWorkbook();
    TWorksheet GetWorksheet(object workbook, string sheetName);
    TWorksheet AddSheet(object workbook, string sheetName, DataTable data);
    TWorksheet AddSheet<T>(object workbook, string sheetName, List<T> data) where T : class;
    void SaveWorkbook(object workbook, string filePath);
    void SaveWorkbook(object workbook, Stream stream);

    // Template functions
    void FillTemplate(string templatePath, string outputPath, Dictionary<string, object> data);
    void FillTemplate(Stream templateStream, Stream outputStream, Dictionary<string, object> data);
}
```

### IExcelService

```csharp
public interface IExcelService
{
    // Import functions
    DataTable ExcelToDataTable(string filePath, string sheetName = null);
    DataTable ExcelToDataTable(Stream stream, string sheetName = null);
    List<T> ExcelToList<T>(string filePath, string sheetName = null) where T : class, new();
    List<T> ExcelToList<T>(Stream stream, string sheetName = null) where T : class, new();
    
    // Import entire workbook as DataSet
    DataSet ExcelToDataSet(string filePath, int? headerRowIndex = 0);
    DataSet ExcelToDataSet(string filePath, IEnumerable<string> sheetNames, int? headerRowIndex = 0);
    DataSet ExcelToDataSet(string filePath, Func<string, int?> headerRowSelector);
    DataSet ExcelToDataSet(string filePath, IEnumerable<string> sheetNames, Func<string, int?> headerRowSelector);
    Task<DataSet> ExcelToDataSetAsync(string filePath, int? headerRowIndex = 0);
    Task<DataSet> ExcelToDataSetAsync(string filePath, IEnumerable<string> sheetNames, int? headerRowIndex = 0);

    // Export functions
    void ListToExcel<T>(List<T> data, string filePath, Action<ExcelStyleOptions> styleAction = null) where T : class;
    void ListToExcel<T>(List<T> data, Stream stream, Action<ExcelStyleOptions> styleAction = null) where T : class;
    void DataTableToExcel(DataTable dataTable, string filePath, Action<ExcelStyleOptions> styleAction = null);
    void DataTableToExcel(DataTable dataTable, Stream stream, Action<ExcelStyleOptions> styleAction = null);
    
    // Export DataSet as multi-sheet Excel
    string DataSetToFile(DataSet dataSet, string filePath, string sheetNamePrefix = "Sheet");

    // Template functions
    void FillTemplate(string templatePath, string outputPath, Dictionary<string, object> data);
    void FillTemplate(Stream templateStream, Stream outputStream, Dictionary<string, object> data);
}
```

## 🎨 Advanced Features

### Import Entire Workbook as DataSet

`ExcelToDataSet` allows you to import an entire Excel workbook or specified worksheets as a `DataSet`, where each worksheet corresponds to a `DataTable`:

```csharp
// 1. Import all worksheets with uniform header row (row 0)
DataSet allSheets = excelService.ExcelToDataSet("workbook.xlsx", headerRowIndex: 0);

// 2. Import only specified worksheets
var selectedSheets = new[] { "UserData", "OrderData", "ProductData" };
DataSet specificSheets = excelService.ExcelToDataSet("workbook.xlsx", selectedSheets, headerRowIndex: 0);

// 3. Specify different header rows for each worksheet
DataSet flexibleHeaders = excelService.ExcelToDataSet("workbook.xlsx", sheetName =>
{
    return sheetName switch
    {
        "UserData" => 0,      // Row 1 is header
        "Financial" => 2,     // Row 3 is header (skip first 2 rows)
        "RawData" => null,    // No header, all rows are data rows
        _ => 0                // Default: row 1 is header
    };
});

// 4. Async import
DataSet result = await excelService.ExcelToDataSetAsync("large-workbook.xlsx", headerRowIndex: 0);

// Access imported data
foreach (DataTable table in result.Tables)
{
    Console.WriteLine($"Worksheet: {table.TableName}, Rows: {table.Rows.Count}, Columns: {table.Columns.Count}");
}
```

**Parameter Description:**
- `headerRowIndex`: Header row index (0-based). Pass `null` to indicate no header, all rows are data rows
- `sheetNames`: List of worksheet names to import. Case-insensitive. Passing empty collection or null will import all worksheets
- `headerRowSelector`: Specify different header rows for each worksheet. Pass worksheet name, return header row index for that worksheet

**For detailed usage and more examples, see [DATASET_USAGE_EXAMPLE.md](./DATASET_USAGE_EXAMPLE.md)**

### Export DataSet as Multi-Sheet Excel

```csharp
DataSet dataSet = new DataSet();
dataSet.Tables.Add(CreateUserTable());
dataSet.Tables.Add(CreateOrderTable());
dataSet.Tables.Add(CreateProductTable());

// Export DataSet, each DataTable becomes a worksheet
string filePath = excelService.DataSetToFile(dataSet, "multi-sheet-workbook.xlsx");
```

### Custom Excel Column Mapping

Use attributes to control how object properties map to Excel columns:

```csharp
public class User
{
    [ExcelColumn(Index = 0, Name = "User ID")]
    public int Id { get; set; }

    [ExcelColumn(Index = 1, Name = "Username")]
    public string Username { get; set; }

    [ExcelColumn(Index = 2, Name = "Email Address", Width = 30)]
    public string Email { get; set; }

    [ExcelColumn(Index = 3, Name = "Registration Date", Format = "yyyy-MM-dd")]
    public DateTime RegistrationDate { get; set; }

    [ExcelColumn(Ignore = true)]
    public string Password { get; set; }
}
```

### Excel Style Customization

```csharp
excelService.ListToExcel(users, "users.xlsx", options =>
{
    // Header style
    options.HeaderBackgroundColor = "#1E90FF"; // Deep blue
    options.HeaderFontColor = "#FFFFFF";       // White
    options.HeaderFontBold = true;
    options.HeaderFontSize = 12;

    // Content style
    options.ContentFontName = "Arial";
    options.ContentFontSize = 10;

    // Alternating row colors
    options.AlternatingRowBackgroundColor = "#F0F8FF"; // Light blue

    // Conditional formatting
    options.AddConditionalFormat(nameof(User.Status), "=Active", cellFormat =>
    {
        cellFormat.BackgroundColor = "#E6FFE6"; // Light green
    });
});
```

### Large Data Processing

```csharp
// Enable batch processing for large datasets
excelService.ListToExcelAsync(
    hugeDataList, 
    "huge_data.xlsx", 
    batchSize: 10000, 
    parallelProcessing: true,
    progress: new Progress<int>(percent => 
    {
        Console.WriteLine($"Completed: {percent}%");
    })
);
```

### Excel Template Filling

```csharp
// Use template and fill data
var templateData = new Dictionary<string, object>
{
    ["ReportTitle"] = "Monthly Sales Report",
    ["GeneratedDate"] = DateTime.Now,
    ["SalesData"] = salesDataList,
    ["TotalAmount"] = salesDataList.Sum(s => s.Amount)
};

excelService.FillTemplate("template.xlsx", "report.xlsx", templateData);
```

## 🧩 Extensibility

### Custom Excel Implementation

```csharp
public class MyCustomExcel : ExcelBase<MyWorkbook, MyWorksheet>
{
    public MyCustomExcel(ExcelOptions options = null) : base(options) { }

    // Implement required methods from base class...
}

// Register custom implementation
services.AddExcel<MyCustomExcel>();
```

### Event Hooks

```csharp
excelService.BeforeExcelExport += (sender, args) =>
{
    Console.WriteLine($"Preparing to export Excel: {args.FilePath}");
};

excelService.AfterExcelImport += (sender, args) =>
{
    Console.WriteLine($"Completed Excel import, total {args.RowCount} rows");
};
```