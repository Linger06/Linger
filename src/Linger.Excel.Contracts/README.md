# Linger Excel Framework

A unified, efficient, and extensible Excel operation framework that supports multiple Excel library implementations

## 🚀 Features Overview

- **Unified Interface** - Multiple underlying implementations, developers don't need to worry about specific details
- **Automatic Type Mapping** - Seamless data conversion between Excel and objects
- **DataSet Support** - Import/export entire workbook as DataSet, supports multi-sheet operations
- **Dependency Injection Friendly** - Supports .NET Core/ASP.NET Core dependency injection
- **High-Performance Design** - Batch processing, parallel processing, and performance monitoring
- **True Async Support** - Async file I/O + Task.Run for CPU-intensive operations
- **Flexible Configuration** - Rich options configuration system
- **Extensibility** - Easy to customize and extend
- **Cross-Platform Compatible** - Supports .NET Framework 4.7.2+, .NET Standard 2.0+, .NET 8+, .NET 9+, .NET 10+

## 📦 Supported Excel Implementations

| Implementation | Package Name | Features |
|----------------|--------------|----------|
| **NPOI** | `Linger.Excel.Npoi` | No Office required, supports .xls and .xlsx |
| **EPPlus** | `Linger.Excel.EPPlus` | High performance, supports richer Excel features |
| **ClosedXML** | `Linger.Excel.ClosedXML` | User-friendly API, good performance |

## 🏗️ Architecture Design

```
┌─────────────────┐
│  IExcelService  │ ◄──── Non-generic interface, provides basic Excel operations
└────────┬────────┘
         │
         │inherits
         ▼
┌─────────────────────┐
│ IExcel<TWorksheet>  │ ◄──── Generic interface, inherits IExcelService and provides advanced operations (with Action delegates)
└────────┬────────────┘
         │
         │implements
         ▼
┌─────────────────────────────┐
│AbstractExcelService<T1,T2>  │ ◄──── Abstract base class, implements common logic and backward compatibility methods
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
│(NpoiExcel, EPPlusExcel, etc)│       
└─────────────────────────────┘
```

**Design Highlights:**
- **Interface Inheritance**: `IExcel<TWorksheet>` inherits from `IExcelService`, automatically gaining all basic methods
- **Method Overloading**: Export methods provide advanced customization via `Action<TWorksheet>` parameters without affecting basic usage
- **Clear Hierarchy**: Basic operations (IExcelService) → Advanced operations (IExcel) → Concrete implementations

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

    public List<User> ImportUsers(string filePath)
    {
        return _excelService.ExcelToList<User>(filePath) ?? new List<User>();
    }

    public string ExportUsers(List<User> users, string filePath)
    {
        return _excelService.CollectionToExcel(users, filePath, "UserList");
    }
    
    public async Task<string> ExportUsersAsync(List<User> users, string filePath)
    {
        // True async file I/O
        return await _excelService.CollectionToExcelAsync(users, filePath, "UserList");
    }
}
```

## 📝 Core Interfaces

### IExcelService - Basic Interface

Provides all fundamental Excel import/export functionality:

```csharp
public interface IExcelService
{
    #region Import Functions
    
    // Import single worksheet as DataTable
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    
    // Import single worksheet as object list
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    
    // Import entire workbook as DataSet (multiple overloads)
    DataSet? ExcelToDataSet(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    // Async imports - True async file I/O
    Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    Task<DataSet?> ExcelToDataSetAsync(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> ExcelToDataSetAsync(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    // Async Stream processing - Virtual methods, subclasses can override for true async
    Task<DataTable?> StreamToDataTableAsync(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<List<T>?> StreamToListAsync<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    Task<DataSet?> StreamToDataSetAsync(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> StreamToDataSetAsync(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    #endregion

    #region Export Functions
    
    // Export to Excel file
    string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");
    string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = "Sheet");
    string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;
    
    // Export to memory stream
    MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "") where T : class;
    MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "");
    
    // Async exports
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;
    
    // Create template
    MemoryStream CreateExcelTemplate<T>() where T : class, new();
    
    #endregion
}
```

### IExcel<TWorksheet> - Advanced Interface

Inherits from `IExcelService` and provides advanced customization with `Action<TWorksheet>` delegates:

```csharp
public interface IExcel<out TWorksheet> : IExcelService where TWorksheet : class
{
    #region Advanced Export Functions - Support Custom Operations
    
    // Export with custom cell and style operations
    string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, 
        Action<TWorksheet>? styleAction = null);
        
    string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = "Sheet",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, 
        Action<TWorksheet>? styleAction = null);
        
    string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, 
        Action<TWorksheet>? styleAction = null) where T : class;
        
    MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, 
        Action<TWorksheet>? styleAction = null) where T : class;
        
    MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, 
        Action<TWorksheet>? styleAction = null);
        
    // Async exports
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, 
        Action<TWorksheet>? styleAction = null);
        
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, 
        Action<TWorksheet>? styleAction = null) where T : class;
    
    #endregion
}
```

**Interface Usage Recommendations:**
- **Basic Scenarios**: Use `IExcelService` for most import/export needs
- **Advanced Customization**: Use `IExcel<TWorksheet>` when you need custom cell styles, merged cells, etc.
- **Dependency Injection**: Both interfaces can be injected; `IExcel<TWorksheet>` instances can be upcast to `IExcelService`

**Async Implementation Notes:**
- ✅ **File I/O**: Uses true async (`FileStream` with `useAsync: true`)
- ⚠️ **Excel Processing**: Uses `Task.Run` to wrap sync methods (library limitation)
- 🔧 **Extensible**: Subclasses can override `StreamToXXXAsync` methods for custom async implementations

## 🎨 Advanced Features

### Import Entire Workbook as DataSet

`ExcelToDataSet` allows you to import an entire Excel workbook or specified worksheets as a `DataSet`, where each worksheet corresponds to a `DataTable`:

```csharp
// 1. Import all worksheets with uniform header row (row 0)
DataSet? allSheets = excelService.ExcelToDataSet("workbook.xlsx", headerRowIndex: 0);

// 2. Import only specified worksheets
var selectedSheets = new[] { "UserData", "OrderData", "ProductData" };
DataSet? specificSheets = excelService.ExcelToDataSet("workbook.xlsx", selectedSheets, headerRowIndex: 0);

// 3. Specify different header rows for each worksheet
DataSet? flexibleHeaders = excelService.ExcelToDataSet("workbook.xlsx", sheetName =>
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
DataSet? result = await excelService.ExcelToDataSetAsync("large-workbook.xlsx", headerRowIndex: 0);

// 5. Backward compatibility mode - compatible with old NPOIHelper.ImportExcelToDs method
// Supports comma-separated worksheet name strings
DataSet? compatibleResult = excelService.ExcelToDataSet("workbook.xlsx", "Sheet1,Sheet2,Sheet3", headerRowIndex: 0);
DataSet? asyncCompatible = await excelService.ExcelToDataSetAsync("workbook.xlsx", "UserData, OrderData", headerRowIndex: 1);

// Access imported data
if (result is not null)
{
    foreach (DataTable table in result.Tables)
    {
        Console.WriteLine($"Worksheet: {table.TableName}, Rows: {table.Rows.Count}, Columns: {table.Columns.Count}");
    }
}
```

**Parameter Description:**
- `headerRowIndex`: Header row index (0-based). All worksheets use the same header row
- `sheetNames`: 
  - `IEnumerable<string>?` - List of worksheet names to import. Pass `null` or empty collection to import all worksheets
  - `string?` - Comma-separated worksheet name string (backward compatible). Automatically handles whitespace, pass `null` to import all worksheets
- `headerRowSelector`: Specify different header rows for each worksheet. Pass worksheet name, return header row index for that worksheet
- `addEmptyRow`: Whether to include empty rows. Default is `false`

**Backward Compatibility:**
The framework provides fully compatible method signatures with the legacy `NPOIHelper.ImportExcelToDs`, allowing migration without modifying existing code.

### Export DataSet as Multi-Sheet Excel

```csharp
DataSet dataSet = new DataSet();
dataSet.Tables.Add(CreateUserTable());
dataSet.Tables.Add(CreateOrderTable());
dataSet.Tables.Add(CreateProductTable());

// Export DataSet, each DataTable becomes a worksheet
string filePath = excelService.DataSetToExcel(dataSet, "multi-sheet-workbook.xlsx");
```

### Advanced Export - Custom Cell Operations

Use the advanced overload methods in `IExcel<TWorksheet>` interface to customize cell operations during export:

```csharp
// Inject IExcel<TWorksheet> instead of IExcelService
public class AdvancedExcelService
{
    private readonly IExcel<ISheet> _npoiExcel; // NPOI's worksheet type is ISheet

    public AdvancedExcelService(IExcel<ISheet> npoiExcel)
    {
        _npoiExcel = npoiExcel;
    }

    public string ExportWithCustomStyle(List<User> users, string filePath)
    {
        return _npoiExcel.CollectionToExcel(users, filePath, "UserList", "User Data Report",
            // Custom cell operations
            action: (sheet, properties) =>
            {
                // Access native ISheet object for advanced operations
                var row = sheet.GetRow(0);
                row.Height = 500; // Set header row height
                
                // Merge cells
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, properties.Length - 1));
            },
            // Custom style operations
            styleAction: (sheet) =>
            {
                // Set column widths
                sheet.SetColumnWidth(0, 3000);
                sheet.SetColumnWidth(1, 5000);
                
                // Freeze panes
                sheet.CreateFreezePane(0, 2);
            }
        );
    }
}
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
// Use IExcel<TWorksheet> advanced overloads for style customization
public class StyledExcelService
{
    private readonly IExcel<ISheet> _npoiExcel;

    public StyledExcelService(IExcel<ISheet> npoiExcel)
    {
        _npoiExcel = npoiExcel;
    }

    public string ExportWithStyles(List<User> users, string filePath)
    {
        return _npoiExcel.CollectionToExcel(users, filePath, "UserData", "User Information",
            styleAction: (sheet) =>
            {
                // Create styles
                var workbook = sheet.Workbook;
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                
                // Header style
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 12;
                headerFont.Color = IndexedColors.White.Index;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = IndexedColors.Blue.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                
                // Apply to header row
                var headerRow = sheet.GetRow(1); // Row 2 is data header
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    headerRow.GetCell(i)?.SetCellStyle(headerStyle);
                }
                
                // Auto-size columns
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
            }
        );
    }
}
```

### Large Data Processing

```csharp
// Async export of large datasets
var largeDataList = GetLargeDataSet(); // Assume 100k records

// Use true async I/O
string filePath = await excelService.CollectionToExcelAsync(
    largeDataList, 
    "huge_data.xlsx", 
    "DataExport"
);

Console.WriteLine($"Export completed: {filePath}");
```

### Excel Template Filling

> Note: Template functionality implementation depends on the chosen Excel library. Please refer to the specific implementation library's documentation.

## 🧩 Extensibility

### Custom Excel Implementation

```csharp
public class MyCustomExcel : AbstractExcelService<MyWorkbook, MyWorksheet>
{
    public MyCustomExcel(ExcelOptions? options = null) : base(options) { }

    // Implement abstract methods...
    protected override MyWorkbook CreateWorkbookInternal() { /* ... */ }
    protected override MyWorksheet GetWorksheetInternal(MyWorkbook workbook, string sheetName) { /* ... */ }
    // More method implementations...
}

// Register custom implementation
services.AddSingleton<IExcelService, MyCustomExcel>();
services.AddSingleton<IExcel<MyWorksheet>, MyCustomExcel>();
```