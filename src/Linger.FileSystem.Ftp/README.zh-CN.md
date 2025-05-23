# Linger.FileSystem.Ftp

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.FileSystem.Ftp 是 Linger FileSystem 抽象的一个实现，提供 FTP 文件操作支持。它使用 FluentFTP 库提供稳健的、具有重试能力的 FTP 客户端，用于常见的文件操作，如上传、下载、列表和删除文件。

## 安装

```bash
dotnet add package Linger.FileSystem.Ftp
```

## 功能特点

- FTP 文件操作（上传、下载、列表、删除）
- 可配置的重试策略，适用于不稳定的网络
- 超时配置
- 与其他 Linger.FileSystem 组件无缝集成
- 支持多个 .NET 框架（net9.0、net8.0、netstandard2.0）

## 基本用法

### 创建 FTP 文件系统实例

```csharp
// 创建远程 FTP 系统设置
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000, // 15 秒
    OperationTimeout = 60000   // 60 秒
};

// 配置重试选项
var retryOptions = new RetryOptions
{
    MaxRetryCount = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// 创建 FTP 文件系统
using var ftpSystem = new FtpFileSystem(settings, retryOptions);

// 连接到服务器
ftpSystem.Connect();

// 使用文件系统
if (ftpSystem.FileExists("/remote/path/file.txt"))
{
    // 下载文件
    var fileContent = ftpSystem.ReadAllText("/remote/path/file.txt");
    
    // 处理文件内容
    Console.WriteLine(fileContent);
}

// 如果目录不存在则创建
ftpSystem.CreateDirectoryIfNotExists("/remote/path/new-directory");

// 上传文件
ftpSystem.WriteAllText("/remote/path/new-file.txt", "你好，世界！");

// 完成后断开连接
ftpSystem.Disconnect();
```

### 异步操作

该库还为所有操作提供异步方法：

```csharp
// 异步连接
await ftpSystem.ConnectAsync();

// 异步检查文件是否存在
if (await ftpSystem.FileExistsAsync("/remote/path/file.txt"))
{
    // 异步下载文件
    var fileContent = await ftpSystem.ReadAllTextAsync("/remote/path/file.txt");
    
    // 处理文件内容
    Console.WriteLine(fileContent);
}

// 完成后异步断开连接
await ftpSystem.DisconnectAsync();
```

## 与依赖注入集成

```csharp
// 在您的启动类中
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFileSystem>(provider => {
        var settings = new RemoteSystemSetting
        {
            Host = "ftp.example.com",
            Port = 21,
            UserName = "username",
            Password = "password",
            ConnectionTimeout = 15000,
            OperationTimeout = 60000
        };
        
        var retryOptions = new RetryOptions
        {
            MaxRetryCount = 3,
            DelayMilliseconds = 1000,
            MaxDelayMilliseconds = 5000
        };
        
        return new FtpFileSystem(settings, retryOptions);
    });
}
```

## 依赖项

- [FluentFTP](https://github.com/robinrodricks/FluentFTP)：现代 FTP 客户端库
- [Linger.FileSystem](https://github.com/yourusername/Linger/tree/main/src/Linger.FileSystem)：核心抽象库

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
