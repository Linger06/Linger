# Linger.Configuration

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

一个适用于 .NET 应用程序的轻量级配置辅助库。

## 概述

Linger.Configuration 提供了实用工具和扩展，以简化 .NET 应用程序中的配置管理。它提供了从各种来源访问和绑定配置设置的一致方法，重点是强类型配置。

## 功能特点

- 基于单例模式的简单配置访问
- 配置设置的强类型支持
- IConfiguration 的扩展方法
- 支持 JSON 配置文件
- 兼容依赖注入
- 跨平台支持

## 安装

```bash
dotnet add package Linger.Configuration
```

## 使用方法

### 基本用法

```csharp
// 访问配置单例
var config = AppConfig.Instance.Config;

// 获取配置值
string connectionString = config.GetConnectionString("DefaultConnection");
int timeoutSeconds = config.GetValue<int>("AppSettings:TimeoutSeconds");

// 绑定到强类型对象
var smtpSettings = config.GetSection("SmtpSettings").Get<SmtpSettings>();
```

### 强类型配置

```csharp
public class AppSettings
{
    public string ApplicationName { get; set; }
    public int CacheTimeoutMinutes { get; set; }
    public bool EnableLogging { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; }
    public string LoggingConnection { get; set; }
}

// 绑定配置
var appSettings = config.Get<AppSettings>();
Console.WriteLine($"应用名称: {appSettings.ApplicationName}");
Console.WriteLine($"缓存超时: {appSettings.CacheTimeoutMinutes} 分钟");
```

### 与依赖注入一起使用

```csharp
// 在 Startup.cs 或 Program.cs 中
public void ConfigureServices(IServiceCollection services)
{
    // 添加配置
    services.AddSingleton<IConfiguration>(AppConfig.Instance.Config);
    
    // 注册强类型选项
    services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
    
    // 使用选项模式
    services.AddTransient<IMyService, MyService>();
}

// 在服务中
public class MyService : IMyService
{
    private readonly AppSettings _settings;
    
    public MyService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }
    
    public void DoSomething()
    {
        // 使用设置
        if (_settings.EnableLogging)
        {
            // 记录日志
        }
    }
}
```

### 辅助方法

```csharp
// 获取带默认值的类型化值
int timeout = AppSettingsHelper.GetValue("TimeoutSeconds", 30);

// 获取连接字符串
string connStr = AppSettingsHelper.GetConnectionString("DefaultConnection");

// 获取复杂配置对象
var emailSettings = AppSettingsHelper.GetSection<EmailSettings>("EmailConfiguration");
```

## 依赖项

- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Configuration.Json
- Linger (核心实用工具)

## 许可证

本项目根据 Linger 项目提供的许可条款授权。
