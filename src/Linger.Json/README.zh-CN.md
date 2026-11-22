# Linger.Json

> 2.0 迁移说明见 [Linger.Json 迁移指南](MIGRATION.zh-CN.md)，其他包的破坏性变更见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

`Linger.Json` 是从 `Linger.Utils` 拆分出的可选 JSON 包，提供 `System.Text.Json` 转换器、`DataTable` JSON 扩展和可复用的 `JsonSerializerOptions` 工厂。

## 安装

```shell
dotnet add package Linger.Json
```

升级到 2.0 时，使用 `JsonDefaults`、`DateTimeConverter`、`DateTimeNullConverter`、`DataTableJsonConverter`、`DataSetConverter`、`JsonStringConverter`、`string.ToDataTable()`、`DataTable.ToJsonString()` 或 `JsonElement.JsonElementToDataTable()` 的项目需要添加 `Linger.Json` 引用。

## DataTable JSON 扩展

```csharp
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using System.Text.Json;

DataTable? table = "[{\"Name\":\"Ada\",\"Age\":37}]".ToDataTable();
string json = table.ToJsonString();

using var document = JsonDocument.Parse(json);
DataTable inferredTable = document.RootElement.JsonElementToDataTable();
```

格式仍为行对象数组，不附带列类型信息：

- 默认裁剪列名首尾空白；若会产生空键或重名，则受影响的列保留原名。
- `byte[]` 单元格（含 `object` 列中的值）写为 Base64 字符串。读取时不猜测字符串是否为二进制；已知列类型时请显式解码。
- 整数优先读为 `long`，必要时使用 `ulong`。普通小数仍为 `double`；若 `double` 的往返文本损失十进制精度，则保留可表示的 `decimal` 值。混合推断类型的列使用 `object`，不再把所有单元格强制转为 `double`。此格式不保证任意精度数值无损。

## JSON 序列化配置

`JsonDefaults` 提供统一的 JSON 序列化选项配置，用于 HTTP 客户端和 ASP.NET Core WebAPI 应用程序。

## 概述

`JsonDefaults` 提供了集中式的 JSON 序列化选项,遵循严进宽出原则:

- 响应/输入选项 CreateResponseOptions: 宽松配置,用于反序列化 API 响应
- 请求/输出选项 CreateRequestOptions: 严格配置,用于序列化 API 请求  
- WebAPI 配置方法 ApplyDefaultConfiguration: 为 WebAPI 优化,平衡输入宽容性和输出优化

## 在 ASP.NET Core WebAPI 中使用

### 用于控制器

在 Program.cs 或 Startup.cs 中添加:

```
using Linger.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => 
        JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions));

var app = builder.Build();
```

### 用于 Minimal API

```
using Linger.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options=>
    JsonDefaults.ApplyDefaultConfiguration(options.SerializerOptions));

var app = builder.Build();
```

### 手动配置

如果需要进一步自定义:

```
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions);
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
```

### 直接配置 JsonSerializerOptions

```
using Linger.Json;

var webApiOptions = new JsonSerializerOptions();
JsonDefaults.ApplyDefaultConfiguration(webApiOptions);

var responseOptions = JsonDefaults.CreateResponseOptions();
var requestOptions = JsonDefaults.CreateRequestOptions();
```

## 方法详解

### 响应选项 - CreateResponseOptions() 

- 接受数字字符串(例如 "123" 可以反序列化为 123)
- 处理循环引用
- 保留 null 值
- 自定义日期/时间转换器
- DataTable 支持
- 驼峰命名属性(Web 默认值)

### 请求选项 - CreateRequestOptions()

- 规范的 JSON 格式(数字不带引号)
- 忽略 null 值(减少载荷大小)
- 自定义日期/时间转换器
- 驼峰命名属性(Web 默认值)
- 仅包含必要的转换器(更轻量)

### WebAPI 配置 - ApplyDefaultConfiguration()

- 接受数字字符串(输入宽容性)
- 处理循环引用
- 忽略 null 值(输出优化)
- 自定义日期/时间转换器
- DataTable 支持
- 驼峰命名属性(Web 默认值)

## 优势

1. 一致性: HTTP 客户端和 WebAPI 之间的相同 JSON 行为
2. 可维护性: JSON 配置的单一事实来源
3. 最佳实践: 遵循严格输出宽松输入原则
4. 互操作性: 处理常见的 API 变体(数字字符串等)

## 内置转换器

以下自定义转换器包含在配置中:

- DateTimeConverter: 处理 DateTime 序列化/反序列化
- DateTimeNullConverter: 处理可空 DateTime
- DataTableJsonConverter: 在 DataTable 和 JSON 之间转换

## 相关类型

- JsonDefaults - JSON 默认配置工具类
- DateTimeConverter - DateTime 序列化转换器
- DateTimeNullConverter - 可空 DateTime 转换器
- DataTableJsonConverter - DataTable 序列化转换器
- StandardHttpClient - 使用这些选项的标准 HTTP 客户端实现
