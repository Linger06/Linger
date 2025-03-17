# Linger Excel Framework

<div align="center">

![Linger Excel Framework](https://img.shields.io/badge/Linger-Excel%20Framework-brightgreen)

一个统一、高效、可扩展的Excel操作框架，支持多种Excel库实现

</div>

## 🚀 特性概览

- **统一接口** - 多种底层实现，开发者无需关心具体细节
- **自动类型映射** - 无缝转换Excel与对象之间的数据
- **依赖注入友好** - 支持.NET Core/ASP.NET Core依赖注入
- **高性能设计** - 批处理、并行处理以及性能监控
- **异步支持** - 全方位异步API支持
- **灵活配置** - 丰富的选项配置系统
- **可扩展性** - 易于自定义和扩展
- **跨平台兼容** - 支持.NET Standard 2.0+、.NET Core 3.1+、.NET 5+

## 📦 支持的Excel实现

| 实现库 | 包名 | 特点 |
|-------|------|-----|
| **NPOI** | `Linger.Excel.Npoi` | 无需Office，支持.xls和.xlsx |
| **EPPlus** | `Linger.Excel.EPPlus` | 高性能，支持更丰富的Excel功能 |
| **ClosedXML** | `Linger.Excel.ClosedXML` | 友好易用的API，性能良好 |

## 🏗️ 架构设计

```
┌─────────────────┐
│  IExcelService  │ ◄──── 非泛型接口，基本Excel操作
└────────┬────────┘
         │
         │实现
         ▼
┌─────────────────┐
│IExcel<TWorksheet>│ ◄──── 泛型接口，高级Excel操作
└────────┬────────┘
         │
         │实现
         ▼
┌─────────────────────────────┐
│AbstractExcelService<T1,T2>  │ ◄──── 抽象基类，公共逻辑
└────────────┬────────────────┘
             │
             │继承
             ▼
┌─────────────────────────────┐
│ ExcelBase<TWorkbook,TSheet> │ ◄──── Excel实现基类，更多常用逻辑
└────────────┬────────────────┘
             │
             │继承
             ▼
┌─────────────────────────────┐
│     具体实现类(如NpoiExcel)   │ ◄──── 具体Excel库实现
└─────────────────────────────┘
```

## 🔧 安装与配置

### 安装包

```bash
# 安装核心接口包
dotnet add package Linger.Excel.Contracts

# 安装具体实现包(选择其一)
dotnet add package Linger.Excel.Npoi      # 使用NPOI实现
dotnet add package Linger.Excel.EPPlus    # 使用EPPlus实现
dotnet add package Linger.Excel.ClosedXML # 使用ClosedXML实现
```

### 配置服务

```csharp
// 程序启动配置(Program.cs或Startup.cs)
public void ConfigureServices(IServiceCollection services)
{
    // 配置选项
    services.AddSingleton(new ExcelOptions {
        EnablePerformanceMonitoring = true,
        PerformanceThreshold = 500, // 记录超过500ms的操作
        ParallelProcessingThreshold = 10000,
        UseBatchWrite = true,
        BatchSize = 5000,
        AutoFitColumns = true
    });
    
    // 注册服务(选择一种实现)
    services.AddScoped<IExcelService, NpoiExcel>();
    // services.AddScoped<IExcelService, EPPlusExcel>();
    // services.AddScoped<IExcelService, ClosedXmlExcel>();
    
    // 如果需要高级功能，可以注册泛型接口
    services.AddScoped<IExcel<ISheet>, NpoiExcel>(); // NPOI
    // services.AddScoped<IExcel<ExcelWorksheet>, EPPlusExcel>(); // EPPlus
    // services.AddScoped<IExcel<IXLWorksheet>, ClosedXmlExcel>(); // ClosedXML
}
```

## 📋 基本使用

### 模型定义

使用 `ExcelColumn` 特性指定Excel列映射:

```csharp
public class User
{
    [ExcelColumn(ColumnName = "用户ID", Index = 0)]
    public int Id { get; set; }
    
    [ExcelColumn(ColumnName = "用户名", Index = 1)]
    public string Username { get; set; }
    
    [ExcelColumn(ColumnName = "邮箱", Index = 2)]
    public string Email { get; set; }
    
    [ExcelColumn(ColumnName = "注册日期", Index = 3)]
    public DateTime RegisterDate { get; set; }
    
    // 没有标注特性的属性，在用户使用了ExcelColumn时不会被导出
    public bool IsActive { get; set; }
}
```

### 导入Excel数据

```csharp
// 注入服务
private readonly IExcelService _excelService;

public MyService(IExcelService excelService)
{
    _excelService = excelService;
}

// 从文件导入
public List<User> ImportUsers(string filePath)
{
    // 同步方式导入
    var users = _excelService.ExcelToList<User>(filePath);
    return users ?? new List<User>();
}

// 从文件异步导入
public async Task<List<User>> ImportUsersAsync(string filePath)
{
    var users = await _excelService.ExcelToListAsync<User>(filePath);
    return users ?? new List<User>();
}

// 从上传的文件流导入
public List<User> ImportFromStream(Stream stream)
{
    // 指定工作表和表头行索引
    return _excelService.ConvertStreamToList<User>(stream, 
        sheetName: "用户数据",  // 可选，默认使用第一个工作表
        headerRowIndex: 1,     // 可选，指定表头在第2行(索引从0开始)
        addEmptyRow: false     // 可选，是否包含空行
    ) ?? new List<User>();
}

// 导入为DataTable
public DataTable ImportRawData(string filePath)
{
    return _excelService.ExcelToDataTable(filePath) ?? new DataTable();
}
```

### 导出Excel数据

```csharp
// 导出到文件
public string ExportToFile(List<User> users)
{
    return _excelService.ListToFile(
        users,                  // 数据源
        "users.xlsx",           // 文件路径
        "用户列表",              // 工作表名称(可选)
        "用户数据导出报表"        // 标题(可选)
    );
}

// 导出到内存流(适用于Web下载)
public IActionResult DownloadExcel(List<User> users)
{
    using var stream = _excelService.ConvertCollectionToMemoryStream(
        users, "用户列表", "用户数据导出");
        
    return File(
        stream.ToArray(), 
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        "users.xlsx"
    );
}

// 导出DataTable
public string ExportDataTable(DataTable dataTable)
{
    return _excelService.DataTableToFile(
        dataTable, 
        "output.xlsx", 
        "数据表",
        "数据表导出"
    );
}

// 异步导出
public async Task<string> ExportAsync(List<User> users)
{
    return await _excelService.ListToFileAsync(
        users, "users.xlsx", "用户列表", "用户数据导出报表");
}
```

### 创建模板

```csharp
// 创建Excel导入模板
public IActionResult GetImportTemplate()
{
    using var stream = _excelService.CreateExcelTemplate<User>();
    
    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        "用户导入模板.xlsx"
    );
}
```

## 🛠️ 高级功能

### 自定义样式和格式

使用泛型接口可以应用自定义样式:

```csharp
// 需要注入泛型接口
private readonly IExcel<IXLWorksheet> _xlExcel; // ClosedXML实现示例

// 导出带自定义样式的Excel
public string ExportWithStyles(List<Product> products)
{
    return _xlExcel.ListToFile(
        products,
        "products.xlsx",
        "产品列表",
        "产品数据",
        // 自定义操作函数
        (worksheet, properties) => {
            // 例如，添加合计行
            var lastRow = worksheet.LastRowUsed().RowNumber();
            var row = worksheet.Row(lastRow + 1);
            row.Cell(1).Value = "合计";
            
            // 计算数量合计
            var sumCell = row.Cell(4); // 假设数量在第4列
            sumCell.FormulaA1 = $"SUM(D2:D{lastRow})";
        },
        // 自定义样式函数
        worksheet => {
            // 为整个工作表应用样式
            worksheet.Style.Font.FontName = "微软雅黑";
            
            // 突出显示特定列
            var priceColumn = worksheet.Column(3); // 假设价格在第3列
            priceColumn.Style.NumberFormat.Format = "#,##0.00";
            priceColumn.Style.Font.FontColor = XLColor.Blue;
        }
    );
}
```

### 处理大数据量

```csharp
// 创建针对大数据量优化的选项
var options = new ExcelOptions
{
    // 启用性能监控
    EnablePerformanceMonitoring = true,
    PerformanceThreshold = 1000,
    
    // 并行处理配置
    ParallelProcessingThreshold = 5000, // 超过5000行启用并行处理
    
    // 批处理配置
    UseBatchWrite = true,
    BatchSize = 2000
};

// 使用这些选项创建服务
var excelService = new NpoiExcel(options, logger);

// 分批处理大数据导入
public async Task ProcessLargeImport(string filePath)
{
    var data = await _excelService.ExcelToListAsync<Product>(filePath);
    if (data == null) return;
    
    // 分批处理导入的数据
    const int batchSize = 1000;
    for (int i = 0; i < data.Count; i += batchSize)
    {
        var batch = data.Skip(i).Take(batchSize).ToList();
        await _productService.ImportBatchAsync(batch);
    }
}
```

### 多语言/全球化

```csharp
// 创建支持不同区域设置的选项
var options = new ExcelOptions
{
    StyleOptions = new ExcelStyleOptions
    {
        DataStyle = new DataStyle
        {
            // 根据不同地区配置日期格式
            DateFormat = culture.DateTimeFormat.ShortDatePattern,
            
            // 根据不同地区配置数字格式
            DecimalFormat = culture.NumberFormat.NumberDecimalPattern,
            IntegerFormat = culture.NumberFormat.NumberPattern[0]
        }
    }
};
```

## ⚙️ 配置选项

### ExcelOptions

主要配置选项:

```csharp
var options = new ExcelOptions
{
    // 性能监控
    EnablePerformanceMonitoring = true, // 启用性能监控
    PerformanceThreshold = 500,         // 记录超过500ms的操作
    
    // 大数据处理
    ParallelProcessingThreshold = 10000, // 超过此行数启用并行处理
    UseBatchWrite = true,                // 使用批量写入
    BatchSize = 5000,                    // 每批处理的行数
    
    // 布局相关
    AutoFitColumns = true,               // 自动调整列宽
    
    // 样式配置
    StyleOptions = new ExcelStyleOptions
    {
        TitleStyle = new TitleStyle
        {
            FontName = "Arial",
            FontSize = 16,
            Bold = true,
            BackgroundColor = "#4472C4",
            FontColor = "#FFFFFF"
        },
        
        HeaderStyle = new HeaderStyle
        {
            FontName = "Arial",
            FontSize = 12,
            Bold = true,
            BackgroundColor = "#D9E1F2",
            FontColor = "#000000"
        },
        
        DataStyle = new DataStyle
        {
            FontName = "Arial",
            FontSize = 11,
            DateFormat = "yyyy-MM-dd",
            DecimalFormat = "#,##0.00",
            IntegerFormat = "#,##0"
        }
    }
};
```

## 📊 属性映射机制

### 使用ExcelColumn特性

```csharp
public class Product
{
    // 基础属性映射
    [ExcelColumn(ColumnName = "产品编号", Index = 0)]
    public string Code { get; set; }
    
    // 控制列顺序
    [ExcelColumn(ColumnName = "产品名称", Index = 1)]
    public string Name { get; set; }
    
    // 映射到Excel中不同名称的列
    [ExcelColumn(ColumnName = "单价", Index = 2)]
    public decimal Price { get; set; }
    
    // 不使用ExcelColumn的属性(当其他属性使用了ExcelColumn时)
    // 此属性不会参与Excel导出，但是会参与导入(如果列名匹配)
    public string Description { get; set; }
}
```

### 映射行为说明

1. **导入Excel至对象**:
   - 先尝试将Excel列名映射到带有`ExcelColumn.ColumnName`特性的属性
   - 再尝试直接匹配属性名称(忽略大小写)
   - 只有可写属性(有public set访问器)会被映射

2. **导出对象至Excel**:
   - 如果类中至少有一个属性使用了`ExcelColumn`特性，则只导出带有特性的属性
   - 如果类中没有属性使用`ExcelColumn`特性，则导出所有可读属性
   - 使用`Index`属性控制列顺序，未指定索引的按声明顺序排序

## ❓ 常见问题

### Q: 如何处理不同的Excel文件格式(.xls和.xlsx)?
> 框架会自动处理格式差异，你只需要选择适合的实现库。NPOI同时支持.xls和.xlsx，而EPPlus和ClosedXML主要支持.xlsx。

### Q: 如何映射复杂类型(如嵌套对象)?
> 目前框架主要支持基本类型和常用.NET类型的映射。对于复杂类型，可以通过自定义转换器或扁平化模型来处理。

### Q: 性能问题：处理大文件时内存占用过高?
> 使用流式导入，配置批处理选项，并考虑分批处理数据而不是一次性全部加载到内存中。

### Q: 如何处理Excel中的合并单元格?
> 导入时框架会处理基本的合并单元格情况，但复杂的合并单元格可能需要使用高级API和自定义逻辑处理。

### Q: 日期格式在不同地区显示不一致?
> 使用`StyleOptions.DataStyle.DateFormat`配置统一的日期格式，或基于当前区域设置动态配置格式。

## ✨ 最佳实践

1. **使用依赖注入**: 通过DI容器注入服务，而不是直接实例化
2. **定义专用DTO**: 为导入/导出场景创建专用模型，避免直接使用领域实体
3. **异常处理**: 所有方法都可能因格式错误等原因抛出异常，确保妥善处理
4. **资源管理**: 使用using语句确保MemoryStream等资源被正确释放
5. **批处理大数据**: 处理大量数据时，考虑分批处理而非一次性处理全部数据
6. **性能优化**: 根据数据量调整并行处理和批处理参数

## 📄 许可证

MIT License
