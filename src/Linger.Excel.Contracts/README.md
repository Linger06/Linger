# Linger Excel 框架

## 概述

Linger Excel 是一个高度抽象的 Excel 操作框架，支持多种 Excel 库实现（如 NPOI、EPPlus、ClosedXML）。框架提供统一的接口，使开发者无需关心底层实现细节，便可轻松进行 Excel 的导入导出操作。

## 特性

- 支持 .NET Standard 2.0+，.NET 8.0+ 和 .NET 9.0
- 统一接口，多种实现
- 自动类型转换
- 灵活的属性映射
- 异步支持
- 性能监控
- 自定义样式和格式

## 架构

```
IExcelService（非泛型接口）
      ↑
IExcel<TWorksheet>（泛型接口）
      ↑
AbstractExcelService<TWorkbook, TWorksheet>（抽象服务基类）
      ↑
ExcelBase<TWorkbook, TWorksheet>（Excel实现基类）
      ↑
具体实现类（如NPOIExcel, EPPlusExcel, ClosedXmlExcel）
```

## 使用方法

### 1. 安装

```bash
# 安装核心包
dotnet add package Linger.Excel.Contracts

# 根据需要安装具体实现包
dotnet add package Linger.Excel.NPOI      # 使用NPOI实现
dotnet add package Linger.Excel.EPPlus    # 使用EPPlus实现
dotnet add package Linger.Excel.ClosedXML # 使用ClosedXML实现
```

### 2. 注册服务

```csharp
// 在Program.cs或Startup.cs中注册
public void ConfigureServices(IServiceCollection services)
{
    // 注册Excel服务 - 选择一种实现
    
    // 选项1: 注册NPOI实现
    services.AddScoped<IExcelService, NpoiExcel>();
    
    // 选项2: 注册EPPlus实现
    // services.AddScoped<IExcelService, EPPlusExcel>();
    
    // 选项3: 注册ClosedXML实现
    // services.AddScoped<IExcelService, ClosedXmlExcel>();
    
    // 配置选项
    services.AddSingleton(new ExcelOptions {
        EnablePerformanceMonitoring = true,
        PerformanceThreshold = 500, // 记录超过500ms的操作
        ParallelProcessingThreshold = 10000,
        UseBatchWrite = true,
        BatchSize = 5000
    });
    
    // 如果需要使用泛型接口
    services.AddScoped<IExcel<ISheet>, NpoiExcel>(); // NPOI
    // services.AddScoped<IExcel<ExcelWorksheet>, EPPlusExcel>(); // EPPlus
    // services.AddScoped<IExcel<IXLWorksheet>, ClosedXmlExcel>(); // ClosedXML
    
    // ... 其他服务注册
}
```

### 3. 导入 Excel 数据

```csharp
// 在控制器或服务中使用
public class ImportService
{
    private readonly IExcelService _excelService;
    
    public ImportService(IExcelService excelService)
    {
        _excelService = excelService;
    }
    
    // 从文件导入
    public List<User> ImportFromFile(string filePath)
    {
        return _excelService.ExcelToList<User>(filePath);
    }
    
    // 从流导入
    public List<User> ImportFromStream(Stream stream)
    {
        return _excelService.ConvertStreamToList<User>(stream);
    }
    
    // 异步导入
    public async Task<List<User>> ImportFromFileAsync(string filePath)
    {
        return await _excelService.ExcelToListAsync<User>(filePath);
    }
}
```

### 4. 导出 Excel 数据

```csharp
// 导出为文件
public string ExportToFile(List<User> users, string filePath)
{
    return _excelService.ListToFile(users, filePath, "用户列表", "用户数据");
}

// 导出到内存流（用于Web下载）
public IActionResult DownloadExcel(List<User> users)
{
    var stream = _excelService.ConvertCollectionToMemoryStream(users, "用户列表", "用户数据");
    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
}

// 导出并应用自定义样式
public string ExportWithStyles(List<Order> orders, string filePath)
{
    return _excelService.ListToFile(
        orders, 
        filePath, 
        "订单列表", 
        "订单数据",
        (worksheet, properties) => {
            // 自定义列处理
        },
        worksheet => {
            // 自定义样式处理
        }
    );
}
```

### 5. 高级用法 - 使用泛型接口

```csharp
// 注入特定实现的泛型接口
public class AdvancedExcelService
{
    private readonly IExcel<IXLWorksheet> _xlExcelService; // ClosedXML
    
    public AdvancedExcelService(IExcel<IXLWorksheet> xlExcelService)
    {
        _xlExcelService = xlExcelService;
    }
    
    // 使用特定实现的高级功能
    public byte[] ExportWithAdvancedFormatting(List<Product> products)
    {
        using var stream = _xlExcelService.ConvertCollectionToMemoryStream(
            products, 
            "产品目录", 
            "产品清单", 
            (worksheet, properties) => {
                // 此处可以直接访问IXLWorksheet的高级特性
            }
        );
        
        return stream.ToArray();
    }
}
```

## 数据模型注解

使用 `ExcelColumn` 特性可以自定义映射规则：

```csharp
public class User
{
    [ExcelColumn(ColumnName = "用户编号", Index = 0)]
    public int Id { get; set; }
    
    [ExcelColumn(ColumnName = "用户名", Index = 1)]
    public string Username { get; set; }
    
    [ExcelColumn(ColumnName = "电子邮箱", Index = 2)]
    public string Email { get; set; }
    
    [ExcelColumn(ColumnName = "注册日期", Index = 3)]
    public DateTime RegisterDate { get; set; }
    
    // 导入时会映射，但导出时不会包含此属性（除非没有任何属性使用ExcelColumn特性）
    public bool IsActive { get; set; }
    
    // 不需要在Excel中显示的属性，可以不设置为公开属性或设置为只写属性
    private string InternalNote { get; set; }
}
```

### 映射行为说明

1. **导入Excel到对象**：
   - 优先将Excel列名映射到带有`ExcelColumn`特性的属性上（使用`ColumnName`值）
   - 然后尝试直接将列名映射到属性名称相同的属性上
   - 映射时忽略大小写
   - 只有可写属性（有public setter）才会被映射

2. **导出对象到Excel**：
   - 如果至少有一个属性使用了`ExcelColumn`特性，则**只导出**那些带有该特性的属性
   - 如果没有任何属性使用`ExcelColumn`特性，则导出所有公开的可读属性
   - 可以通过`Index`属性控制导出的列顺序

3. **列顺序控制**：
   - 使用`ExcelColumn`特性的`Index`属性可以控制列的显示顺序
   - 较小的`Index`值将会排在前面
   - 没有指定`Index`的属性将按声明顺序排列

## 创建模板

通过 `CreateExcelTemplate<T>()` 方法可以创建仅包含列名的模板文件：

```csharp
// 创建用户导入模板
public IActionResult GetUserTemplate()
{
    var stream = _excelService.CreateExcelTemplate<User>();
    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "用户导入模板.xlsx");
}
```

## 配置选项

通过 `ExcelOptions` 可以配置Excel操作的行为：

```csharp
public class ExcelOptions
{
    /// <summary>
    /// 启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;
    
    /// <summary>
    /// 性能监控阈值(毫秒)，超过此阈值将记录日志
    /// </summary>
    public int PerformanceThreshold { get; set; } = 500;
    
    /// <summary>
    /// 并行处理阈值，超过此行数将启用并行处理
    /// </summary>
    public int ParallelProcessingThreshold { get; set; } = 10000;
    
    /// <summary>
    /// 是否使用批量写入
    /// </summary>
    public bool UseBatchWrite { get; set; } = true;
    
    /// <summary>
    /// 批处理大小
    /// </summary>
    public int BatchSize { get; set; } = 5000;
    
    /// <summary>
    /// 是否自动调整列宽
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;
    
    /// <summary>
    /// 样式配置选项
    /// </summary>
    public ExcelStyleOptions StyleOptions { get; set; } = new ExcelStyleOptions();
}
```

框架还提供了 `ExcelStyleOptions` 配置样式相关行为：

```csharp
public class ExcelStyleOptions
{
    /// <summary>
    /// 标题样式配置
    /// </summary>
    public TitleStyle TitleStyle { get; set; } = new();
    
    /// <summary>
    /// 表头样式配置
    /// </summary>
    public HeaderStyle HeaderStyle { get; set; } = new();
    
    /// <summary>
    /// 数据行样式配置
    /// </summary>
    public DataStyle DataStyle { get; set; } = new();
    
    /// <summary>
    /// 是否显示网格线
    /// </summary>
    public bool ShowGridlines { get; set; } = true;
}

// 标题样式配置
public class TitleStyle
{
    public string FontName { get; set; } = "Arial";
    public int FontSize { get; set; } = 14;
    public bool Bold { get; set; } = true;
    public string BackgroundColor { get; set; } = "#D0D0D0";
    public string FontColor { get; set; } = "#000000";
}

// 表头样式配置
public class HeaderStyle
{
    public string FontName { get; set; } = "Arial";
    public int FontSize { get; set; } = 11;
    public bool Bold { get; set; } = true;
    public string BackgroundColor { get; set; } = "#E0E0E0";
    public string FontColor { get; set; } = "#000000";
}

// 数据行样式配置
public class DataStyle
{
    public string FontName { get; set; } = "Arial";
    public int FontSize { get; set; } = 10;
    public string IntegerFormat { get; set; } = "#,##0";
    public string DecimalFormat { get; set; } = "#,##0.00";
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}
```

使用方式：

```csharp
// 配置Excel选项
var excelOptions = new ExcelOptions 
{
    EnablePerformanceMonitoring = true,
    ParallelProcessingThreshold = 10000,
    
    // 样式配置
    StyleOptions = new ExcelStyleOptions 
    {
        TitleStyle = new TitleStyle 
        {
            Bold = true,
            FontSize = 16,
            BackgroundColor = "#4472C4"
        },
        
        HeaderStyle = new HeaderStyle
        {
            Bold = true,
            BackgroundColor = "#D9E1F2"
        },
        
        DataStyle = new DataStyle
        {
            DateFormat = "yyyy-MM-dd"
        }
    }
};

// 使用选项创建服务
var excelService = new ClosedXmlExcel(excelOptions);
```

## 常见问题

### 1. Excel列名和属性名不匹配怎么办？
使用 `ExcelColumn` 特性的 `ColumnName` 属性指定Excel中的列名。

### 2. 如何处理大文件导入导出？
框架内置了多种大文件处理机制，可通过`ExcelOptions`配置：

```csharp
// 配置Excel选项以优化大文件处理
var options = new ExcelOptions
{
    // 超过10000行启用并行处理
    ParallelProcessingThreshold = 10000,
    
    // 启用批量写入
    UseBatchWrite = true,
    BatchSize = 5000
};

// 使用这些选项创建服务
var excelService = new NpoiExcel(options);
```

对于代码中的分批处理：
```csharp
// 分批次导入处理
public async Task ImportLargeFile(string filePath)
{
    const int batchSize = 1000;
    
    // 对于特别大的文件，可以使用流式读取避免一次性加载全部数据
    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    var data = _excelService.ConvertStreamToList<MyEntity>(stream);
    
    // 分批处理
    for (int i = 0; i < data.Count; i += batchSize)
    {
        var batch = data.Skip(i).Take(batchSize).ToList();
        await ProcessBatchAsync(batch);
    }
}
```

### 3. 多个工作表如何处理？
当前框架主要处理单个工作表，可以通过指定工作表名称来选择要操作的工作表。

## 扩展自己的实现

如果需要自定义Excel实现，需要以下步骤：

1. 引用 `Linger.Excel.Contracts` 包
2. 创建一个继承自 `ExcelBase<TWorkbook, TWorksheet>` 的类
3. 实现所有抽象方法

```csharp
public class MyExcelService : ExcelBase<MyWorkbook, MyWorksheet>
{
    public MyExcelService(ExcelOptions? options = null, ILogger<MyExcelService>? logger = null)
        : base(options, logger)
    {
    }
    
    // 实现所有抽象方法...
}
```

## 最佳实践

1. **使用依赖注入**：总是通过依赖注入使用Excel服务，而不是直接创建实例
2. **模型设计**：为导入导出数据设计专用的DTO模型，并使用ExcelColumn特性
3. **异常处理**：所有导入导出方法都可能抛出异常，确保适当处理
4. **资源释放**：使用using语句确保MemoryStream等资源被正确释放
5. **大文件处理**：处理大型Excel文件时，考虑使用流式处理或分批导入

## 版本历史

### v0.3.0
- 重构样式配置选项，改为更清晰的分层结构
- 优化并行处理和批量写入功能
- 支持自动调整列宽

### v0.2.0
- 添加异步方法
- 改进性能监控

### v0.1.0
- 项目初始化

## 许可证

此框架基于 MIT 许可证。
