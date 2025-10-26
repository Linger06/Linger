# Linger.FileSystem.Ftp

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
await ftpSystem.ConnectAsync();

// 使用文件系统
if (await ftpSystem.FileExistsAsync("/remote/path/file.txt"))
{
    // 下载文件
    var result = ftpSystem.DownloadFileAsync("/remote/path/file.txt","/local/file.txt");    
    if (result.Success)
    {
        logger.LogInformation("文件下载成功");
        var fullFilePath = result.FullFilePath;
    }
}

// 如果目录不存在则创建
await ftpSystem.CreateDirectoryIfNotExistsAsync("/remote/path/new-directory");

// 上传文件
await ftpSystem.UploadFileAsync("/local/path/file.txt", "/remote/path/new-file.txt");

// 完成后断开连接
await ftpSystem.DisconnectAsync();
```

## 与依赖注入集成

```csharp
builder.Services.AddSingleton<IRemoteFileSystem>(provider => {
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
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000,
        MaxDelayMilliseconds = 5000
    };

    return new FtpFileSystem(settings, retryOptions);
});
```

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
