# Linger.FileSystem.Ftp

> 破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

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
var settings = new FtpFileSystemOptions
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
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// 创建 FTP 文件系统
using var ftpSystem = new FtpFileSystem(settings, retryOptions);

// 上传文件
await using var stream = File.OpenRead("./local/file.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

if (result.Success)
{
    Console.WriteLine($"上传成功: {result.FilePath}");
}

// 下载文件
var downloadResult = await ftpSystem.DownloadFileAsync("/remote/path/file.txt", "C:/Downloads/file.txt");

if (downloadResult.Success)
{
    var downloadedBytes = await ftpSystem.GetFileSizeAsync("/remote/path/file.txt");
    Console.WriteLine($"已下载 {downloadedBytes} 字节");
}

```

### FTP 客户端编码

`FtpFileSystemOptions.Encoding` 用于配置 FTP 客户端处理协议文本和远程路径名时使用的编码；未设置时默认使用 UTF-8：

```csharp
var settings = new FtpFileSystemOptions
{
    Host = "ftp.example.com",
    UserName = "username",
    Password = "password",
    Encoding = System.Text.Encoding.UTF8
};
```

此选项不控制文件内容编码。读写文本文件时，应通过 `GetReaderAsync` 或 `GetWriterAsync` 的编码参数单独指定。

### 文件上传方法

```csharp
// 方法 1: 从流上传到完整文件路径
await using var stream = File.OpenRead("local.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// 方法 2: 上传本地文件到完整远程路径
result = await ftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);
```

## 与依赖注入集成

```csharp
builder.Services.AddTransient<IRemoteFileSystem>(provider => {
    var settings = new FtpFileSystemOptions
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
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000,
        MaxDelayMilliseconds = 5000
    };

    return new FtpFileSystem(settings, retryOptions);
});
```

`FtpFileSystem` 会在首次操作时自动连接，并在每个实例中复用一个客户端连接。不要在同一实例上并发执行操作，
也不要在其他操作运行期间修改工作目录。

`FtpFileSystem` 实现了 `IAsyncRemoteFileSystem`。需要异步释放时，请使用具体类型或该能力接口；
通用的 `IRemoteFileSystem` 契约提供同步释放。

## 最佳实践

1. **连接管理**：始终使用 `using` 语句或确保正确释放 FTP 连接
2. **错误处理**：为 `FtpException` 和 `TimeoutException` 实现特定的异常处理
3. **超时设置**：根据网络条件和文件大小配置适当的超时
4. **重试逻辑**：使用指数退避进行重试尝试，避免压垮服务器
5. **大文件**：对于大于可用内存的文件使用流式操作
6. **安全性**：使用配置管理或密钥保管库安全地存储连接凭据
7. **性能优化**：为频繁操作调整缓冲区大小

## 依赖项

- [FluentFTP](https://github.com/robinrodricks/FluentFTP)：现代 FTP 客户端库
- [Linger.FileSystem](https://github.com/Linger06/Linger/tree/main/src/Linger.FileSystem)：核心抽象库

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
