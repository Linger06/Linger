# Linger.Excel.Npoi

## 概述

Linger.Excel.Npoi 是基于强大的 [NPOI](https://github.com/nissl-lab/npoi) 库实现的 Linger.Excel.Contracts 中定义的 Excel 操作接口。该包提供了高性能的 Excel 文件操作功能，无需安装 Microsoft Office。

## 功能特点

- 通过简单直观的 API 读取和写入 Excel 文件（.xlsx 和 .xls 格式）
- 将 Excel 数据转换为强类型对象集合，反之亦然
- 支持数据表导入导出
- 基于模板的 Excel 生成
- 高级样式和格式化选项
- 单元格公式支持和计算
- 多工作表管理
- 全面的错误处理
- 支持传统 Excel 格式（.xls）

## 安装

```shell
dotnet add package Linger.Excel.Npoi
```

## 使用方法

### 基本设置

```csharp
// 在 DI 容器中注册
services.AddExcelNpoi(options => 
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.EnableFormulaEvaluation = true;
});

// 或直接创建实例
var excel = new NpoiExcel(new ExcelOptions 
{
    DefaultDateFormat = "yyyy-MM-dd",
    EnableFormulaEvaluation = true
});
```

### 读取 Excel 文件

```csharp
// 将 Excel 转换为 List<T>
var users = excel.ExcelToList<UserModel>("users.xlsx");

// 将 Excel 转换为 DataTable
var dataTable = excel.ExcelToDataTable("data.xlsx", sheetName: "Sheet1");

// 从流中读取
using var stream = File.OpenRead("data.xlsx");
var results = excel.ExcelToList<ResultModel>(stream);
```

### 写入 Excel 文件

```csharp
// 将 List<T> 转换为 Excel 文件
var users = GetUsers(); // 您的数据源
excel.ListToExcel(users, "users.xlsx");

// 将 DataTable 转换为 Excel 文件
var dataTable = GetDataTable(); // 您的数据源
excel.DataTableToExcel(dataTable, "data.xlsx");

// 导出到流（用于 Web 下载等）
using var memoryStream = new MemoryStream();
excel.ListToExcel(products, memoryStream);
return File(memoryStream.ToArray(), 
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
    "products.xlsx");
```

### 高级功能

```csharp
// 多工作表操作
var workbook = excel.CreateWorkbook();
excel.AddSheet(workbook, "Users", users);
excel.AddSheet(workbook, "Products", products);
excel.SaveWorkbook(workbook, "company_data.xlsx");

// 样式设置
excel.ListToExcel(salesData, "sales_report.xlsx", styleOptions => 
{
    styleOptions.HeaderBackgroundColor = "#1E90FF";
    styleOptions.HeaderFontColor = "#FFFFFF";
    styleOptions.AlternatingRowBackgroundColor = "#F0F8FF";
});

// 公式计算
var options = new ExcelOptions
{
    EnableFormulaEvaluation = true
};
var excelWithFormulas = new NpoiExcel(options);
var calculatedData = excelWithFormulas.ExcelToList<CalculationModel>("formulas.xlsx");
```

## NPOI 特有功能

### 支持传统 Excel 格式

```csharp
// 读取 .xls 文件
var legacyData = excel.ExcelToList<LegacyData>("old_data.xls");

// 写入 .xls 文件
excel.ListToExcel(legacyData, "legacy_export.xls");
```

### 单元格注释

```csharp
// 访问 NPOI 特定功能
IWorkbook workbook = (IWorkbook)excel.CreateWorkbook();
ISheet sheet = workbook.CreateSheet("Comments");
IRow row = sheet.CreateRow(0);
ICell cell = row.CreateCell(0);
cell.SetCellValue("带注释的单元格");

// 添加注释
IDrawing drawing = sheet.CreateDrawingPatriarch();
IComment comment = drawing.CreateCellComment(new XSSFClientAnchor(0, 0, 0, 0, 0, 0, 5, 5));
comment.String = new XSSFRichTextString("这是一个注释");
comment.Author = "系统";
cell.CellComment = comment;

excel.SaveWorkbook(workbook, "with_comments.xlsx");
```

## 依赖项

- NPOI (v2.7.3+)
- Linger.Excel.Contracts

## 相关包

- [Linger.Excel.Contracts](../Linger.Excel.Contracts/) - 核心接口和抽象
- [Linger.Excel.ClosedXML](../Linger.Excel.ClosedXML/) - 使用 ClosedXML 库的替代实现

## 目标框架

- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
