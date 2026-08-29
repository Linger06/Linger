# Linger.FileSystem

> 破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

Linger.FileSystem 为本地存储、FTP 和 SFTP 提供文件系统抽象。文件内容传输共享异步契约；本地元数据使用同步 API，远程元数据保留异步协议 API。

## 项目结构

Linger.FileSystem解决方案包含以下NuGet包：

- **Linger.FileSystem**: 核心库，提供共享传输契约、本地同步能力契约和本地文件系统实现
- **Linger.FileSystem.Ftp**: FTP文件系统实现，基于FluentFTP
- **Linger.FileSystem.Sftp**: SFTP文件系统实现，基于SSH.NET

## 安装方式

```
# 安装核心库
dotnet add package Linger.FileSystem

# 安装FTP支持
dotnet add package Linger.FileSystem.Ftp

# 安装SFTP支持
dotnet add package Linger.FileSystem.Sftp
```

## 主要特点

- **能力契约**: `IFileTransfer` 统一真实异步传输；本地同步 API 与远程异步 API 分别由专用接口定义
- **支持多种文件系统**: 包括本地文件系统、FTP和SFTP
- **真实异步 I/O**: 文件内容传输保持异步，不使用 `Task.Run` 包装同步文件系统 API
- **自动重试**: 内置重试机制，可以配置重试次数和延迟，提高操作可靠性
- **连接管理**: 按需自动连接，并复用连接直到文件系统实例被释放
- **多种命名规则**: 支持MD5、UUID和普通命名规则
- **流式上传优化**: 本地文件系统使用 `IncrementalHash` 和 `ArrayPool<byte>` 实现内存友好的大文件处理，支持任意大小文件
- **批量操作进度报告**: 通过 `IProgress<BatchProgress>` 实时跟踪批量上传、下载和删除操作的进度

## 支持的.NET版本

- .NET Framework 4.6.2+
- .NET Standard 2.0+
- .NET 5+

## 快速入门

### 使用本地文件系统

```csharp
// 创建本地文件系统实例
var localFs = new LocalFileSystem("C:/Storage");

// 上传文件
using var fileStream = File.OpenRead("test.txt");
var result = await localFs.UploadAsync(fileStream, "uploads/destination-file.txt", true);

// 检查上传结果
if (result.Success)
{
    Console.WriteLine($"文件上传成功: {result.FilePath}");
}
```

### 批量操作（本地）

本地批量操作按输入顺序执行。上传和下载保留真正的异步文件 I/O；由于 `File.Delete` 没有异步 API，删除保持同步执行。批量 API 统一提供进度报告、取消、目标冲突检查和成功/失败汇总。

示例：

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage"
};

var localFs = new LocalFileSystem(options);

// 批量上传到根目录下的相对目录（复制到 C:/Storage/uploads）
var uploadResult = await localFs.UploadFilesAsync(new[]
{
    "C:/in/a.txt",
    "C:/in/b.txt"
}, "uploads", overwrite: true);
Console.WriteLine($"上传成功: {uploadResult.SucceededFiles.Count}, 失败: {uploadResult.FailedFiles.Count}");

// 批量下载：从根目录下的相对路径复制到指定本地目录
var downloadResult = await localFs.DownloadFilesAsync(new[]
{
    "uploads/a.txt",
    "uploads/b.txt"
}, "C:/downloads", overwrite: true);
Console.WriteLine($"下载成功: {downloadResult.SucceededFiles.Count}, 失败: {downloadResult.FailedFiles.Count}");

// 批量删除：传入根内的相对路径
var deleteResult = localFs.DeleteFiles(new[]
{
    "uploads/a.txt",
    "uploads/b.txt"
});
Console.WriteLine($"删除成功: {deleteResult.SucceededFiles.Count}, 失败: {deleteResult.FailedFiles.Count}");
```

说明：上述批量接口返回 `BatchOperationResult`，其中包含 `SucceededFiles` 与 `FailedFiles`（失败项含错误信息与异常对象）。

### 批量操作进度报告

使用 `IProgress<BatchProgress>` 监控批量操作进度：

```csharp
// 创建进度处理器
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"进度: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"当前文件: {p.CurrentFile}");
    Console.WriteLine($"成功: {p.Succeeded}, 失败: {p.Failed}");
});

// 带进度报告的批量上传
var uploadResult = await localFs.UploadFilesAsync(files, "/uploads", overwrite: true, progress);

// 带进度报告的批量下载
var downloadResult = await localFs.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// 带进度报告的批量删除
var deleteResult = localFs.DeleteFiles(filesToDelete, progress);
```

`BatchProgress` 结构包含：
- `Completed`: 已处理的文件数（每个文件处理完成后报告）
- `Total`: 总文件数
- `CurrentFile`: 刚处理完成的文件路径
- `Succeeded`: 成功的操作数
- `Failed`: 失败的操作数
- `PercentComplete`: 完成百分比 (0-100)

**说明**: 进度报告在每个文件操作*完成后*发送，确保 `Completed` 始终反映准确的计数。

### 使用FTP文件系统

需要先安装Linger.FileSystem.Ftp包:

```
Install-Package Linger.FileSystem.Ftp
```

基本使用示例:

```csharp
using Linger.FileSystem.Ftp;

var ftpOptions = new FtpFileSystemOptions
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password"
};

await using var ftpFs = new FtpFileSystem(ftpOptions);
var result = await ftpFs.UploadAsync(fileStream, "/public_html/test.txt", true);
```

📖 **详细的FTP文档和高级功能，请参阅: [Linger.FileSystem.Ftp README](../Linger.FileSystem.Ftp/README.zh-CN.md)**

### 使用SFTP文件系统

需要先安装Linger.FileSystem.Sftp包:

```
Install-Package Linger.FileSystem.Sftp
```

基本使用示例:

```csharp
using Linger.FileSystem.Sftp;

var sftpOptions = new SftpFileSystemOptions
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    Password = "password"
};

using var sftpFs = new SftpFileSystem(sftpOptions);
var result = await sftpFs.UploadAsync(fileStream, "/home/user/test.txt", true);
```

📖 **详细的SFTP文档和高级功能，请参阅: [Linger.FileSystem.Sftp README](../Linger.FileSystem.Sftp/README.zh-CN.md)**

## 常见操作

### 文件上传

```csharp
// 上传流
using var stream = File.OpenRead("local-file.txt");
var result = await fileSystem.UploadAsync(stream, "uploads/destination-file.txt", true);

// 上传本地文件
result = await fileSystem.UploadFileAsync("local-file.txt", "uploads", true);

// 带取消令牌的上传
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
result = await fileSystem.UploadAsync(
    stream, 
    "uploads/destination-file.txt", 
    true, 
    cts.Token);
```

`FileOperationResult` 不包含文件大小。本地文件系统调用 `GetFileSize`，远程文件系统调用 `GetFileSizeAsync`。

### 文件下载

```csharp
// 下载到流
using var outputStream = new MemoryStream();
var result = await fileSystem.DownloadToStreamAsync("uploads/file.txt", outputStream);

// 下载到本地文件
result = await fileSystem.DownloadFileAsync("uploads/file.txt", "C:/Downloads/downloaded-file.txt", true);

// 带取消令牌的下载
using var cts = new CancellationTokenSource();
result = await fileSystem.DownloadFileAsync(
    "uploads/large-file.zip", 
    "C:/Downloads/large-file.zip", 
    true, 
    cts.Token);
```

### 文件删除

```csharp
var localResult = localFileSystem.Delete("uploads/file-to-delete.txt");
var remoteResult = await remoteFileSystem.DeleteAsync("uploads/file-to-delete.txt");
```

### 目录操作

```csharp
// 本地文件系统使用同步元数据 API
bool localExists = localFileSystem.DirectoryExists("uploads/images");
localFileSystem.CreateDirectoryIfNotExists("uploads/documents");

// 远程文件系统保留真实异步协议 API
bool remoteExists = await remoteFileSystem.DirectoryExistsAsync("uploads/images");
await remoteFileSystem.CreateDirectoryIfNotExistsAsync("uploads/documents");

```

本地文件系统的目录枚举使用同步的 `ListFiles` 和 `ListDirectories`；FTP/SFTP 远程目录枚举使用异步的 `ListFilesAsync` 和 `ListDirectoriesAsync`。

### 流工厂 API

打开本地流是同步操作；流返回后，内容读写仍使用真实异步 I/O：

```csharp
await using var readStream = localFileSystem.OpenRead("data/large-file.bin");
await ProcessLargeFileAsync(readStream);

await using var writeStream = localFileSystem.OpenWrite("output/result.bin", overwrite: true);
await writeStream.WriteAsync(data, cancellationToken);

using var reader = localFileSystem.GetReader("logs/app.log", Encoding.UTF8);
while (await reader.ReadLineAsync() is { } line)
{
    ProcessLine(line);
}

// 使用 StreamWriter 写入文本文件
await using var writer = localFileSystem.GetWriter("output/report.csv", overwrite: true, Encoding.UTF8);
await writer.WriteLineAsync("Name,Value");
await writer.WriteLineAsync("Item1,100");
```

### 元数据查询 API

```csharp
// 本地查询同步完成；远程查询需要协议 I/O
var fileSize = localFileSystem.GetFileSize("uploads/document.pdf");
var remoteFileSize = await remoteFileSystem.GetFileSizeAsync("uploads/document.pdf", cancellationToken);
if (fileSize.HasValue)
{
    Console.WriteLine($"文件大小: {fileSize.Value} 字节");
}
else
{
    Console.WriteLine("文件不存在");
}

// 可通过 IRemoteFileSystem 使用远程元数据和工作目录操作
var lastModified = await remoteFileSystem.GetLastModifiedTimeAsync("uploads/document.pdf", cancellationToken);
await remoteFileSystem.SetWorkingDirectoryAsync("uploads", cancellationToken);
```

## 配置选项

### 本地文件系统选项

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",          // 根目录路径
    DefaultNamingRule = NamingRule.Md5,        // 默认命名规则: Md5、Uuid、Normal
    DefaultOverwrite = false,                  // 是否默认覆盖同名文件
    DefaultUseSequencedName = true,            // 文件名冲突时是否使用序号命名
    ValidationLevel = FileValidationLevel.SizeOnly, // 验证级别: None(不验证)、SizeOnly(仅大小)、Full(完整)
    CleanupOnValidationFailure = true,         // 验证失败时是否清理文件
    UploadBufferSize = 81920,                  // 上传缓冲区大小
    DownloadBufferSize = 81920,                // 下载缓冲区大小
    RetryOptions = new RetryOptions 
    { 
        MaxRetryAttempts = 3, 
        DelayMilliseconds = 1000 
    }
};

var localFs = new LocalFileSystem(options);
```

### 远程文件系统选项

远程配置和连接生命周期属于具体实现包，请参阅 [Linger.FileSystem.Ftp](../Linger.FileSystem.Ftp/README.zh-CN.md) 和 [Linger.FileSystem.Sftp](../Linger.FileSystem.Sftp/README.zh-CN.md)。

## 高级功能

### 本地文件系统高级功能

#### 文件命名规则

本地文件系统支持三种文件命名规则：

- **Normal**: 保持原始文件名
- **Md5**: 使用文件内容的MD5哈希值命名，适合去重场景
- **Uuid**: 使用UUID生成唯一文件名，适合避免冲突

#### 高级上传示例

```csharp
// 使用高级上传功能（指定命名规则）
var uploadedInfo = await localFs.UploadWithNamingAsync(
    stream,
    "source-file.txt",  // 源文件名
    "container1",       // 容器名
    "images",           // 目标路径
    NamingRule.Md5,     // 命名规则: Normal、Md5、Uuid
    false,              // 是否覆盖
    true                // 是否使用序号命名
);

var uploadedFileInfo = await localFs.UploadFileWithNamingAsync(
    "source-file.txt",
    "container1",
    "images",
    NamingRule.Uuid);

// 访问上传后的信息
Console.WriteLine($"文件哈希: {uploadedInfo.HashData}");
Console.WriteLine($"相对路径: {uploadedInfo.FilePath}");
Console.WriteLine($"完整路径: {uploadedInfo.FullFilePath}");
```

## 取消操作支持

所有文件系统操作都支持 `CancellationToken`，实现优雅的取消机制：

```csharp
using var timeoutSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
    cancellationToken,
    timeoutSource.Token);

var result = await fileSystem.UploadAsync(
    stream,
    "uploads/destination-file.txt",
    overwrite: true,
    linkedSource.Token);
```

## 异常处理

```csharp
try
{
    var result = await fileSystem.UploadFileAsync("local.txt", "uploads/remote.txt");
    if (result.Success)
    {
        Console.WriteLine($"上传成功: {result.FilePath}");
    }
    else
    {
        Console.WriteLine($"上传失败: {result.ErrorMessage}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("操作被取消");
}
catch (FileSystemException ex)
{
    Console.WriteLine($"文件系统操作异常: {ex.Message}");
    Console.WriteLine($"操作: {ex.Operation}, 路径: {ex.Path}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部异常: {ex.InnerException.Message}");
    }
}
```

## 重试机制

```csharp
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,                     // 最多重试5次
    DelayMilliseconds = 1000,                 // 初始延迟1秒
    MaxDelayMilliseconds = 30000,             // 最大延迟30秒
    UseExponentialBackoff = true              // 使用指数退避算法
};

var localFs = new LocalFileSystem("C:/Storage", retryOptions);
```

## 性能优化

### 缓冲区大小调优

默认缓冲区大小为 81920 字节（80KB）。对于大文件处理场景，可以适当增大缓冲区以提升吞吐量：

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    UploadBufferSize = 262144,    // 256KB - 适合大文件上传
    DownloadBufferSize = 262144   // 256KB - 适合大文件下载
};
```

更大的缓冲区可提升大文件吞吐量，但会增加当前复制操作的内存占用。

### 批量操作优化

对于需要处理大量文件的场景，可以使用批量 API 统一管理进度、取消、冲突检查和结果汇总：

```csharp
string[] localFiles = Directory.GetFiles("local/directory", "*.txt");
await localFs.UploadFilesAsync(localFiles, "uploads");
```

## 架构设计

### 核心接口层次

```
                    IFileTransfer
                   /             \
      ILocalFileSystem       IRemoteFileSystem
                                      ^
                         IAsyncRemoteFileSystem (FTP)
```

### 核心接口

- **IFileTransfer**: 定义本地和远程共用的真实异步文件内容传输
- **ILocalFileSystem**: 定义同步本地元数据和流工厂，以及异步内容传输
- **IRemoteFileSystem**: 定义远程元数据、流、目录、传输及同步释放能力
- **IAsyncRemoteFileSystem**: 在远程接口基础上增加真实异步释放能力，目前由 `FtpFileSystem` 实现

### 实现类层次

```
                FileSystemBase
                /           \
   LocalFileSystem     RemoteFileSystemBase
                         /         \
               FtpFileSystem    SftpFileSystem
```

### 基础类

- **FileSystemBase**: 复用重试、日志和批量结果辅助逻辑，不声明本地或远程执行契约
- **RemoteFileSystemBase**: 远程文件系统的抽象基类，统一实现同步释放并负责连接闸门资源
- **LocalFileSystem**: 本地文件系统具体实现
- **FtpFileSystem**: FTP文件系统实现，基于FluentFTP库，并支持 `IAsyncRemoteFileSystem`
- **SftpFileSystem**: SFTP文件系统实现，基于SSH.NET库，仅支持同步释放

### 设计模式

该库使用了以下设计模式：

- **能力接口**: 本地和远程共享传输契约，但分别表达同步和异步元数据能力
- **模板方法**: 在FileSystemBase基类中定义算法骨架，子类实现具体步骤
- **适配器模式**: 将FluentFTP和SSH.NET等不同的文件系统API适配到统一接口
- **简单工厂**: 各个文件系统类内部的CreateClient()方法用于创建具体的客户端实例
- **命令模式**: 通过FileOperationResult封装操作结果，统一处理执行状态

### 关键流程

1. **文件上传流程**:
   - 客户端通过 `IFileTransfer.UploadAsync` 发起真实异步内容传输
   - 根据实际文件系统类型执行不同实现 
   - 应用配置的文件命名规则和验证措施
   - 返回统一的FileOperationResult结果

2. **远程系统连接管理**:
   - 使用 `EnsureConnectedAsync()` 自动管理连接
   - 按需自动连接；连接保持到实例被释放

## 贡献

欢迎提交Pull Request和Issue帮助我们改进这个库。

## 许可证

MIT
