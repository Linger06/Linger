# Linger.Excel.ClosedXML

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.Excel.ClosedXML 是基于强大的 [ClosedXML](https://github.com/ClosedXML/ClosedXML) 库实现的 Linger.Excel.Contracts 中定义的 Excel 操作接口。该包提供了高性能的 Excel 文件操作功能，并具有清晰、抽象的 API。

## 功能特点

- 通过简单直观的 API 读取和写入 Excel 文件（.xlsx 格式）
- 将 Excel 数据转换为强类型对象集合，反之亦然
- 支持数据表导入导出
- 基于模板的 Excel 生成
- 高级样式和格式化选项
- 单元格公式支持
- 多工作表管理
- 全面的错误处理

## 安装

```shell
dotnet add package Linger.Excel.ClosedXML
```

## 使用方法

### 基本设置

```csharp
// 在 DI 容器中注册
services.AddExcelClosedXML(options => 
{
    options.DefaultDateFormat = "yyyy-MM-dd";
    options.AutoFitColumns = true;
});

// 或直接创建实例
var excel = new ClosedXmlExcel(new ExcelOptions 
{
    DefaultDateFormat = "yyyy-MM-dd",
    AutoFitColumns = true  
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
```

## 依赖项

- ClosedXML (v0.105.0+)
- Linger.Excel.Contracts

## 相关包

- [Linger.Excel.Contracts](../Linger.Excel.Contracts/) - 核心接口和抽象
- [Linger.Excel.Npoi](../Linger.Excel.Npoi/) - 使用 NPOI 库的替代实现

## 目标框架

- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
