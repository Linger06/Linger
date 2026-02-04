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
- 统一的批量操作接口与并发控制（`MaxDegreeOfParallelism`）

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
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// 创建 FTP 文件系统
using var ftpSystem = new FtpFileSystem(settings, retryOptions);

// 连接到服务器
await ftpSystem.ConnectAsync();

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
    Console.WriteLine($"已下载 {downloadResult.FileSize} 字节");
}

// 完成后断开连接
await ftpSystem.DisconnectAsync();
```

## 并发与批量操作

### 配置并发开关

在批量上传/下载/删除时，FTP 客户端支持通过 `RemoteSystemSetting.MaxDegreeOfParallelism` 控制并发度：

```csharp
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000,
    OperationTimeout = 60000,
    // 并发度：1 表示串行，>1 表示并发执行（每个任务独立连接）
    MaxDegreeOfParallelism = 4
};

var ftp = new FtpFileSystem(settings);
await ftp.ConnectAsync();
```

并发实现说明：当并发度为 1 时，使用单连接串行执行；当并发度大于 1 时，为每个任务创建独立的 `AsyncFtpClient` 连接，确保线程安全并提升吞吐量。

### 统一批量操作接口示例

批量操作返回 `BatchOperationResult`，包含成功与失败的文件列表与错误信息。

```csharp
// 批量上传到指定目录
var localFiles = new[]
{
    "C:/data/a.txt",
    "C:/data/b.txt",
    "C:/data/c.txt"
};

var uploadResult = await ftp.UploadFilesAsync(localFiles, "/remote/uploads", overwrite: true);
Console.WriteLine($"上传成功: {uploadResult.SucceededFiles.Count}, 失败: {uploadResult.FailedFiles.Count}");

// 批量下载到本地目录
var remoteFiles = new[]
{
    "/remote/uploads/a.txt",
    "/remote/uploads/b.txt"
};

var downloadResult = await ftp.DownloadFilesAsync(remoteFiles, "C:/downloads", overwrite: true);
Console.WriteLine($"下载成功: {downloadResult.SucceededFiles.Count}, 失败: {downloadResult.FailedFiles.Count}");

// 批量删除
var deleteResult = await ftp.DeleteFilesAsync(new[]
{
    "/remote/uploads/a.txt",
    "/remote/uploads/b.txt"
});
Console.WriteLine($"删除成功: {deleteResult.SucceededFiles.Count}, 失败: {deleteResult.FailedFiles.Count}");

await ftp.DisconnectAsync();
```

失败项详见 `BatchOperationResult.FailedFiles`，每项包含文件路径、错误消息与异常对象（可能为 null）。

### 批量操作进度报告

您可以使用 `IProgress<BatchProgress>` 参数监控批量操作进度：

```csharp
// 创建进度处理器
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"进度: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"当前文件: {p.CurrentFile}");
    Console.WriteLine($"成功: {p.Succeeded}, 失败: {p.Failed}");
});

// 带进度报告的批量上传
var localFiles = new[] { "C:/data/a.txt", "C:/data/b.txt", "C:/data/c.txt" };
var result = await ftp.UploadFilesAsync(localFiles, "/remote/uploads", overwrite: true, progress);

// 带进度报告的批量下载
var remoteFiles = new[] { "/remote/uploads/a.txt", "/remote/uploads/b.txt" };
var downloadResult = await ftp.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// 带进度报告的批量删除
var deleteResult = await ftp.DeleteFilesAsync(new[] { "/remote/old.txt" }, progress);
```

`BatchProgress` 结构包含：
- `Completed`: 已处理的文件数（每个文件处理完成后报告）
- `Total`: 总文件数
- `CurrentFile`: 刚处理完成的文件路径
- `Succeeded`: 成功的操作数
- `Failed`: 失败的操作数
- `PercentComplete`: 完成百分比 (0-100)

**说明**: 进度报告在每个文件操作*完成后*发送，确保 `Completed` 始终反映准确的计数。

### 连接池与重试配置

```csharp
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    MaxDegreeOfParallelism = 4,
    
    // 连接池空闲超时（可选）
    // 空闲超过此时间的连接将被丢弃并重新创建
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // 批量操作重试设置
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    }
};

var ftp = new FtpFileSystem(settings);
```

重试配置说明：
- `MaxRetryAttempts`: 当单个文件操作失败时，最多重试的次数
- `DelayMilliseconds`: 重试间隔

### 文件上传方法

```csharp
// 方法 1: 从流上传到完整文件路径
await using var stream = File.OpenRead("local.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// 方法 2: 上传本地文件到完整远程路径
result = await ftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);

// 方法 3: 分别指定目录和文件名上传（便于动态命名）
result = await ftpSystem.UploadFileAsync(
    "C:/local/file.txt",           // 本地文件路径
    "/remote/directory",            // 远程目录
    "custom-name.txt",              // 自定义文件名
    overwrite: true
);
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
