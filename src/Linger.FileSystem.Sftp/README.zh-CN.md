# Linger.FileSystem.Sftp

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.FileSystem.Sftp 是 Linger FileSystem 抽象的一个实现，提供 SFTP（SSH 文件传输协议）文件操作支持。它使用 SSH.NET 库提供安全可靠的 SFTP 客户端，支持密码和证书认证方式进行文件操作。

## 安装

```bash
dotnet add package Linger.FileSystem.Sftp
```

## 功能特点

- 通过 SFTP 进行安全的文件操作（上传、下载、列表、删除）
- 支持密码和证书双重认证方式
- 可配置的重试策略，适用于不稳定的网络
- 超时配置
- 与 Linger.FileSystem 抽象无缝集成
- 支持多个 .NET 框架（net9.0、net8.0、netstandard2.0）

## 基本用法

### 使用密码认证创建 SFTP 文件系统实例

```csharp
// 创建远程 SFTP 系统设置，使用密码认证
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
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

// 创建 SFTP 文件系统
using var sftpSystem = new SftpFileSystem(settings, retryOptions);

// 连接到服务器
sftpSystem.Connect();

// 使用文件系统
if (sftpSystem.FileExists("/remote/path/file.txt"))
{
    // 下载文件
    var fileContent = sftpSystem.ReadAllText("/remote/path/file.txt");
    
    // 处理文件内容
    Console.WriteLine(fileContent);
}

// 如果目录不存在则创建
sftpSystem.CreateDirectoryIfNotExists("/remote/path/new-directory");

// 上传文件
sftpSystem.WriteAllText("/remote/path/new-file.txt", "你好，世界！");

// 完成后断开连接
sftpSystem.Disconnect();
```

### 使用证书认证

```csharp
// 创建远程 SFTP 系统设置，使用证书认证
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    CertificatePath = "/path/to/private/key.pem",
    CertificatePassphrase = "optional-passphrase", // 如果私钥有密码保护
    ConnectionTimeout = 15000, // 15 秒
    OperationTimeout = 60000   // 60 秒
};

// 创建使用证书认证的 SFTP 文件系统
using var sftpSystem = new SftpFileSystem(settings);

// 连接并正常使用
sftpSystem.Connect();
// ... 执行操作 ...
sftpSystem.Disconnect();
```

### 异步操作

该库还为所有操作提供异步方法：

```csharp
// 异步连接
await sftpSystem.ConnectAsync();

// 异步检查文件是否存在
if (await sftpSystem.FileExistsAsync("/remote/path/file.txt"))
{
    // 异步下载文件
    var fileContent = await sftpSystem.ReadAllTextAsync("/remote/path/file.txt");
    
    // 处理文件内容
    Console.WriteLine(fileContent);
}

// 完成后异步断开连接
await sftpSystem.DisconnectAsync();
```

## 与依赖注入集成

```csharp
// 在您的启动类中
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFileSystem>(provider => {
        var settings = new RemoteSystemSetting
        {
            Host = "sftp.example.com",
            Port = 22,
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
        
        return new SftpFileSystem(settings, retryOptions);
    });
}
```

## 依赖项

- [SSH.NET](https://github.com/sshnet/SSH.NET)：SSH2 客户端协议的 .NET 实现
- [Linger.FileSystem](https://github.com/yourusername/Linger/tree/main/src/Linger.FileSystem)：核心抽象库

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
