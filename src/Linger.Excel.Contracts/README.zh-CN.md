# Linger Excel Framework

一个统一、高效、可扩展的Excel操作框架，支持多种Excel库实现

## 🚀 特性概览

- **统一接口** - 多种底层实现，开发者无需关心具体细节
- **自动类型映射** - 无缝转换Excel与对象之间的数据
- **DataSet支持** - 导入/导出整个工作簿为DataSet，支持多工作表操作
- **依赖注入友好** - 支持.NET Core/ASP.NET Core依赖注入
- **高性能设计** - 批处理、并行处理以及性能监控
- **真正的异步支持** - 异步文件 I/O + Task.Run 包裹 CPU 密集型操作
- **灵活配置** - 丰富的选项配置系统
- **可扩展性** - 易于自定义和扩展
- **跨平台兼容** - 支持.NET Framework 4.7.2+、.NET Standard 2.0+、.NET 8+、.NET 9+、.NET 10+

## 📦 支持的Excel实现

| 实现库 | 包名 | 特点 |
|-------|------|-----|
| **NPOI** | `Linger.Excel.Npoi` | 无需Office，支持.xls和.xlsx |
| **EPPlus** | `Linger.Excel.EPPlus` | 高性能，支持更丰富的Excel功能 |
| **ClosedXML** | `Linger.Excel.ClosedXML` | 友好易用的API，性能良好 |

## 🏗️ 架构设计

```
┌─────────────────┐
│  IExcelService  │ ◄──── 非泛型接口，提供基础Excel操作
└────────┬────────┘
         │
         │继承
         ▼
┌─────────────────────┐
│ IExcel<TWorksheet>  │ ◄──── 泛型接口，继承IExcelService并提供高级操作(带Action委托自定义)
└────────┬────────────┘
         │
         │实现
         ▼
┌─────────────────────────────┐
│AbstractExcelService<T1,T2>  │ ◄──── 抽象基类，实现公共逻辑和向后兼容方法
└────────────┬────────────────┘
             │
             │继承
             ▼
┌─────────────────────────────┐
│ ExcelBase<TWorkbook,TSheet> │ ◄──── Excel实现基类，更多通用逻辑
└────────────┬────────────────┘
             │
             │继承
             ▼
┌─────────────────────────────┐
│     具体实现类               │ ◄──── 具体Excel库实现
│ (NpoiExcel, EPPlusExcel等)  │       
└─────────────────────────────┘
```

**设计亮点:**
- **接口继承**: `IExcel<TWorksheet>` 继承自 `IExcelService`,自动拥有所有基础方法
- **方法重载**: Export 方法通过增加 `Action<TWorksheet>` 参数实现高级自定义,不影响基础方法使用
- **层次清晰**: 基础操作(IExcelService) → 高级操作(IExcel) → 具体实现

## 🚀 快速入门

### 1. 安装包

```bash
# 安装核心接口包
dotnet add package Linger.Excel.Contracts

# 安装你选择的实现
dotnet add package Linger.Excel.Npoi
# 或者
dotnet add package Linger.Excel.ClosedXML
```

### 2. 注册服务

```csharp
// 在Startup.cs或Program.cs中
// NPOI实现
services.AddExcelNpoi(options =>
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.EnableFormulaEvaluation = true;
});

// 或者使用ClosedXML实现
services.AddExcelClosedXML(options =>
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.AutoFitColumns = true;
});
```

### 3. 在服务中使用

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
        return _excelService.CollectionToExcel(users, filePath, "用户列表");
    }
    
    public async Task<string> ExportUsersAsync(List<User> users, string filePath)
    {
        // 真正的异步文件 I/O
        return await _excelService.CollectionToExcelAsync(users, filePath, "用户列表");
    }
}
```

## 📝 核心接口

### IExcelService - 基础接口

提供所有基础的 Excel 导入/导出功能:

```csharp
public interface IExcelService
{
    #region 导入功能
    
    // 导入单个工作表为 DataTable
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    
    // 导入单个工作表为对象列表
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    
    // 导入整个工作簿为 DataSet (支持多种重载)
    DataSet? ExcelToDataSet(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    // 异步导入 - 真正的异步文件 I/O
    Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    Task<DataSet?> ExcelToDataSetAsync(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> ExcelToDataSetAsync(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    // 异步 Stream 处理 - 虚拟方法,子类可覆盖以提供真正的异步
    Task<DataTable?> StreamToDataTableAsync(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<List<T>?> StreamToListAsync<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    Task<DataSet?> StreamToDataSetAsync(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);
    Task<DataSet?> StreamToDataSetAsync(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);
    
    #endregion

    #region 导出功能
    
    // 导出为 Excel 文件
    string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");
    string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = "Sheet");
    string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;
    
    // 导出为内存流
    MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "") where T : class;
    MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "");
    
    // 异步导出
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;
    
    // 创建模板
    MemoryStream CreateExcelTemplate<T>() where T : class, new();
    
    #endregion
}
```

### IExcel<TWorksheet> - 高级接口

继承自 `IExcelService`,在其基础上提供带 `Action<TWorksheet>` 委托的高级自定义功能:

```csharp
public interface IExcel<out TWorksheet> : IExcelService where TWorksheet : class
{
    #region 高级导出功能 - 支持自定义操作
    
    // 导出时支持自定义单元格和样式操作
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
        
    // 异步导出
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, 
        Action<TWorksheet>? styleAction = null);
        
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, 
        Action<TWorksheet>? styleAction = null) where T : class;
    
    #endregion
}
```

**接口使用建议:**
- **基础场景**: 使用 `IExcelService` 即可满足大部分导入导出需求
- **高级定制**: 需要自定义单元格样式、合并单元格等操作时,使用 `IExcel<TWorksheet>`
- **依赖注入**: 两个接口都可注入,`IExcel<TWorksheet>` 实例可向上转型为 `IExcelService`

**异步实现说明:**
- ✅ **文件 I/O**: 使用真正的异步 (`FileStream` 的 `useAsync: true`)
- ⚠️ **Excel 处理**: 使用 `Task.Run` 包裹同步方法(底层库限制)
- 🔧 **可扩展**: 子类可以覆盖 `StreamToXXXAsync` 方法提供自定义异步实现

## 🎨 高级功能

### 导入整个工作簿为DataSet

`ExcelToDataSet` 允许您将整个 Excel 工作簿或指定的工作表导入为 `DataSet`,每个工作表对应一个 `DataTable`:

```csharp
// 1. 导入所有工作表,统一使用第0行作为表头
DataSet? allSheets = excelService.ExcelToDataSet("workbook.xlsx", headerRowIndex: 0);

// 2. 只导入指定的工作表
var selectedSheets = new[] { "用户数据", "订单数据", "产品数据" };
DataSet? specificSheets = excelService.ExcelToDataSet("workbook.xlsx", selectedSheets, headerRowIndex: 0);

// 3. 为每个工作表指定不同的表头行
DataSet? flexibleHeaders = excelService.ExcelToDataSet("workbook.xlsx", sheetName =>
{
    return sheetName switch
    {
        "用户数据" => 0,      // 第1行是表头
        "财务报表" => 2,      // 第3行是表头(跳过前2行)
        "原始数据" => null,   // 没有表头,将所有行作为数据行
        _ => 0               // 默认第1行是表头
    };
});

// 4. 异步导入
DataSet? result = await excelService.ExcelToDataSetAsync("large-workbook.xlsx", headerRowIndex: 0);

// 5. 向后兼容方式 - 兼容旧版 NPOIHelper.ImportExcelToDs 方法
// 支持逗号分隔的工作表名称字符串
DataSet? compatibleResult = excelService.ExcelToDataSet("workbook.xlsx", "Sheet1,Sheet2,Sheet3", headerRowIndex: 0);
DataSet? asyncCompatible = await excelService.ExcelToDataSetAsync("workbook.xlsx", "用户数据, 订单数据", headerRowIndex: 1);

// 访问导入的数据
if (result is not null)
{
    foreach (DataTable table in result.Tables)
    {
        Console.WriteLine($"工作表: {table.TableName}, 行数: {table.Rows.Count}, 列数: {table.Columns.Count}");
    }
}
```

**参数说明:**
- `headerRowIndex`: 表头行索引(0-based)。所有工作表使用相同的表头行
- `sheetNames`: 
  - `IEnumerable<string>?` - 要导入的工作表名称列表。传入 `null` 或空集合将导入所有工作表
  - `string?` - 逗号分隔的工作表名称字符串(向后兼容)。自动处理空格,传入 `null` 导入所有工作表
- `headerRowSelector`: 为每个工作表指定不同的表头行。传入工作表名称,返回该工作表的表头行索引
- `addEmptyRow`: 是否包含空行。默认 `false`

**向后兼容性:**
框架提供了与旧版 `NPOIHelper.ImportExcelToDs` 完全兼容的方法签名,无需修改现有代码即可迁移。

### 导出DataSet为多工作表Excel

```csharp
DataSet dataSet = new DataSet();
dataSet.Tables.Add(CreateUserTable());
dataSet.Tables.Add(CreateOrderTable());
dataSet.Tables.Add(CreateProductTable());

// 导出DataSet,每个DataTable成为一个工作表
string filePath = excelService.DataSetToExcel(dataSet, "multi-sheet-workbook.xlsx");
```

### 高级导出 - 自定义单元格操作

使用 `IExcel<TWorksheet>` 接口的高级重载方法,可以在导出时自定义单元格操作:

```csharp
// 注入 IExcel<TWorksheet> 而不是 IExcelService
public class AdvancedExcelService
{
    private readonly IExcel<ISheet> _npoiExcel; // NPOI 的工作表类型是 ISheet

    public AdvancedExcelService(IExcel<ISheet> npoiExcel)
    {
        _npoiExcel = npoiExcel;
    }

    public string ExportWithCustomStyle(List<User> users, string filePath)
    {
        return _npoiExcel.CollectionToExcel(users, filePath, "用户列表", "用户数据报表",
            // 自定义单元格操作
            action: (sheet, properties) =>
            {
                // 访问原生 ISheet 对象,进行高级操作
                var row = sheet.GetRow(0);
                row.Height = 500; // 设置表头行高
                
                // 合并单元格
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, properties.Length - 1));
            },
            // 自定义样式操作
            styleAction: (sheet) =>
            {
                // 设置列宽
                sheet.SetColumnWidth(0, 3000);
                sheet.SetColumnWidth(1, 5000);
                
                // 冻结窗格
                sheet.CreateFreezePane(0, 2);
            }
        );
    }
}
```

### 自定义Excel列映射

使用特性来控制如何将对象属性映射到Excel列：

```csharp
public class User
{
    [ExcelColumn(Index = 0, Name = "用户ID")]
    public int Id { get; set; }

    [ExcelColumn(Index = 1, Name = "用户名")]
    public string Username { get; set; }

    [ExcelColumn(Index = 2, Name = "邮箱地址", Width = 30)]
    public string Email { get; set; }

    [ExcelColumn(Index = 3, Name = "注册日期", Format = "yyyy年MM月dd日")]
    public DateTime RegistrationDate { get; set; }

    [ExcelColumn(Ignore = true)]
    public string Password { get; set; }
}
```

### Excel样式自定义

```csharp
// 使用 IExcel<TWorksheet> 的高级重载进行样式自定义
public class StyledExcelService
{
    private readonly IExcel<ISheet> _npoiExcel;

    public StyledExcelService(IExcel<ISheet> npoiExcel)
    {
        _npoiExcel = npoiExcel;
    }

    public string ExportWithStyles(List<User> users, string filePath)
    {
        return _npoiExcel.CollectionToExcel(users, filePath, "用户数据", "用户信息表",
            styleAction: (sheet) =>
            {
                // 创建样式
                var workbook = sheet.Workbook;
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                
                // 表头样式
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 12;
                headerFont.Color = IndexedColors.White.Index;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = IndexedColors.Blue.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                
                // 应用到表头行
                var headerRow = sheet.GetRow(1); // 第2行是数据表头
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    headerRow.GetCell(i)?.SetCellStyle(headerStyle);
                }
                
                // 自动调整列宽
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
            }
        );
    }
}
```

### 大数据处理

```csharp
// 异步导出大数据集
var largeDataList = GetLargeDataSet(); // 假设有10万条数据

// 使用真正的异步 I/O
string filePath = await excelService.CollectionToExcelAsync(
    largeDataList, 
    "huge_data.xlsx", 
    "数据导出"
);

Console.WriteLine($"导出完成: {filePath}");
```

### Excel模板填充

> 注意: 模板功能的具体实现取决于所选的 Excel 库实现。请参考具体实现库的文档。

### 扩展性

### 自定义Excel实现

```csharp
public class MyCustomExcel : ExcelBase<MyWorkbook, MyWorksheet>
{
    public MyCustomExcel(ExcelOptions options = null) : base(options) { }

    // 实现基类要求的方法...
}

// 注册自定义实现
services.AddExcel<MyCustomExcel>();
```

### 事件钩子

```csharp
excelService.BeforeExcelExport += (sender, args) =>
{
    Console.WriteLine($"准备导出Excel: {args.FilePath}");
};

excelService.AfterExcelImport += (sender, args) =>
{
    Console.WriteLine($"已完成导入Excel，共{args.RowCount}行数据");
};
```