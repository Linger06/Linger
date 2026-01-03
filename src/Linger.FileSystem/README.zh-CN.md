# Linger.FileSystem

Linger.FileSystem 是一个统一的文件系统抽象库，提供了对多种文件系统的一致访问接口，包括本地文件系统、FTP和SFTP。通过这个库，您可以使用相同的API操作不同类型的文件系统，简化开发过程，提高代码复用性。

## 项目结构

Linger.FileSystem解决方案包含以下NuGet包：

- **Linger.FileSystem**: 核心库，提供统一接口和本地文件系统实现
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

## 架构设计

### 核心接口层次

```
              IFileSystem
                   │
         IFileSystemOperations
           /            \
ILocalFileSystem    IRemoteFileSystem
```

### 核心接口

- **IFileSystem**: 定义基本文件操作接口
- **IFileSystemOperations**: 统一的文件系统操作接口，继承自 IFileSystem
- **ILocalFileSystem**: 本地文件系统特定接口，扩展了特有功能
- **IRemoteFileSystem**: 远程文件系统连接管理接口

### 实现类层次

```
                FileSystemBase
                /           \
   LocalFileSystem     RemoteFileSystemBase
                         /         \
               FtpFileSystem    SftpFileSystem
```

### 基础类

- **FileSystemBase**: 所有文件系统的抽象基类，实现了IFileSystemOperations接口
- **RemoteFileSystemBase**: 远程文件系统的抽象基类，继承自FileSystemBase，实现了IRemoteFileSystem
- **LocalFileSystem**: 本地文件系统具体实现
- **FtpFileSystem**: FTP文件系统实现，基于FluentFTP库
- **SftpFileSystem**: SFTP文件系统实现，基于SSH.NET库

### 设计模式

该库使用了以下设计模式：

- **策略模式**: 不同文件系统实现相同接口(IFileSystemOperations)但有各自的实现策略
- **模板方法**: 在FileSystemBase基类中定义算法骨架，子类实现具体步骤
- **适配器模式**: 将FluentFTP和SSH.NET等不同的文件系统API适配到统一接口
- **简单工厂**: 各个文件系统类内部的CreateClient()方法用于创建具体的客户端实例
- **命令模式**: 通过FileOperationResult封装操作结果，统一处理执行状态

### 关键流程

1. **文件上传流程**:
   - 客户端调用IFileSystemOperations.UploadAsync
   - 根据实际文件系统类型执行不同实现 
   - 应用配置的文件命名规则和验证措施
   - 返回统一的FileOperationResult结果

2. **远程系统连接管理**:
   - 使用 `EnsureConnectedAsync()` 自动管理连接
   - 自动连接、操作、关闭的生命周期管理

## 主要特点

- **统一接口**: 通过 `IFileSystemOperations` 接口提供的一致API操作不同类型的文件系统
- **支持多种文件系统**: 包括本地文件系统、FTP和SFTP
- **异步操作**: 所有操作都支持异步方法，适用于现代应用程序开发
- **自动重试**: 内置重试机制，可以配置重试次数和延迟，提高操作可靠性
- **连接管理**: 自动处理远程文件系统的连接和断开
- **多种命名规则**: 支持MD5、UUID和普通命名规则
- **流式上传优化**: 本地文件系统使用 `IncrementalHash` 和 `ArrayPool<byte>` 实现内存友好的大文件处理（内存减少 99.99%）
- **批量操作进度报告**: 通过 `IProgress<BatchProgress>` 实时跟踪批量上传、下载和删除操作的进度
- **连接池空闲超时**: 通过 `ConnectionPoolIdleTimeout` 配置自动清理连接池中的空闲连接
- **批量操作重试**: 通过 `BatchRetryOptions` 配置为批量操作中的单个文件提供重试支持

## 支持的.NET版本

- .NET Framework 4.6.2+
- .NET Standard 2.0+
- .NET Core 2.0+/.NET 5+

## 安装

通过NuGet包管理器安装:

```
Install-Package Linger.FileSystem
```

或使用.NET CLI:

```
dotnet add package Linger.FileSystem
```

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

### 并发与批量操作（本地）

本地批量操作支持通过 `LocalFileSystemOptions.MaxDegreeOfParallelism` 配置并发度，用于提升批量复制/删除的吞吐量：

- 值为 1：串行执行（默认），资源占用低；
- 值大于 1：并发执行（内部使用限流），适合大量文件批处理。

示例：

```csharp
// 配置并发度并使用统一批量接口
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    MaxDegreeOfParallelism = 4 // 1 串行，>1 并发
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
var deleteResult = await localFs.DeleteFilesAsync(new[]
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
var uploadResult = await fileSystem.UploadFilesAsync(files, "/uploads", overwrite: true, progress);

// 带进度报告的批量下载
var downloadResult = await fileSystem.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// 带进度报告的批量删除
var deleteResult = await fileSystem.DeleteFilesAsync(filesToDelete, progress);
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

var ftpSetting = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    Type = "FTP"
};

var ftpFs = new FtpFileSystem(ftpSetting);
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

var sftpSetting = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    Password = "password",
    Type = "SFTP"
};

var sftpFs = new SftpFileSystem(sftpSetting);
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
var result = await fileSystem.DeleteAsync("uploads/file-to-delete.txt");
```

### 目录操作

```csharp
// 检查目录是否存在
bool exists = await fileSystem.DirectoryExistsAsync("uploads/images");

// 创建目录
await fileSystem.CreateDirectoryIfNotExistsAsync("uploads/documents");

// 判断路径是否为目录
if (await fileSystem.IsDirectoryAsync("uploads/images"))
{
    Console.WriteLine("路径是一个目录");
}
```

### 流工厂 API

高效的流式操作，无需将整个文件加载到内存：

```csharp
// 打开文件进行读取（返回原始 Stream）
await using var readStream = await fileSystem.OpenReadAsync("data/large-file.bin", cancellationToken);
await ProcessLargeFileAsync(readStream);

// 打开文件进行写入
await using var writeStream = await fileSystem.OpenWriteAsync("output/result.bin", overwrite: true, cancellationToken);
await writeStream.WriteAsync(data, cancellationToken);

// 使用 StreamReader 读取文本文件
using var reader = await fileSystem.GetReaderAsync("logs/app.log", Encoding.UTF8, cancellationToken);
while (await reader.ReadLineAsync() is { } line)
{
    ProcessLine(line);
}

// 使用 StreamWriter 写入文本文件
await using var writer = await fileSystem.GetWriterAsync("output/report.csv", overwrite: true, Encoding.UTF8, cancellationToken);
await writer.WriteLineAsync("Name,Value");
await writer.WriteLineAsync("Item1,100");
```

### 元数据查询 API

```csharp
// 获取文件大小（如果文件不存在则返回 null）
var fileSize = await fileSystem.GetFileSizeAsync("uploads/document.pdf", cancellationToken);
if (fileSize.HasValue)
{
    Console.WriteLine($"文件大小: {fileSize.Value} 字节");
}
else
{
    Console.WriteLine("文件不存在");
}
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
    ValidationLevel = FileValidationLevel.Full, // 验证级别: None(不验证)、SizeOnly(仅大小)、Full(完整)
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

### 远程文件系统设置

```csharp
var remoteSetting = new RemoteSystemSetting
{
    Host = "example.com",                      // 主机地址
    Port = 21,                                 // 端口 (FTP默认21，SFTP默认22)
    UserName = "username",                     // 用户名
    Password = "password",                     // 密码
    Type = "FTP",                              // 类型: "FTP" 或 "SFTP"
    ConnectionTimeout = 30000,                 // 连接超时(毫秒)
    OperationTimeout = 60000,                  // 操作超时(毫秒)
    MaxDegreeOfParallelism = 4,                // 批量操作并发度
    
    // 连接池空闲超时（空闲超过此时间的连接将被重新创建）
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // 批量操作重试设置
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    },
    
    // SFTP特定设置
    CertificatePath = "",                      // 证书路径
    CertificatePassphrase = ""                 // 证书口令
};
```

## 高级功能

### 本地文件系统高级功能

```csharp
// 使用高级上传功能
var uploadedInfo = await localFs.UploadAsync(
    stream,
    "source-file.txt",  // 源文件名
    "container1",       // 容器名
    "images",           // 目标路径
    NamingRule.Md5,     // 命名规则
    false,              // 是否覆盖
    true                // 是否使用序号命名
);

// 访问上传后的信息
Console.WriteLine($"文件哈希: {uploadedInfo.HashData}");
Console.WriteLine($"相对路径: {uploadedInfo.FilePath}");
Console.WriteLine($"完整路径: {uploadedInfo.FullFilePath}");
```

### FTP和SFTP高级功能

对于FTP和SFTP的高级功能，如：
- 批量文件操作
- 目录列表和操作
- 工作目录管理
- 证书认证 (SFTP)
- 自定义超时配置

请参阅专门的文档：
- 📖 **[Linger.FileSystem.Ftp 文档](../Linger.FileSystem.Ftp/README.zh-CN.md)**
- 📖 **[Linger.FileSystem.Sftp 文档](../Linger.FileSystem.Sftp/README.zh-CN.md)**

## 连接管理

```csharp
// 方式1: 使用using语句自动管理连接
using (var ftpFs = new FtpFileSystem(remoteSetting))
{
    // 操作自动处理连接和断开
    await ftpFs.UploadFileAsync("local.txt", "/remote/path");
}

// 方式2: 手动管理连接
try
{
    await ftpFs.ConnectAsync();
    // 执行多个操作...
    await ftpFs.UploadFileAsync("file1.txt", "/remote");
    await ftpFs.UploadFileAsync("file2.txt", "/remote");
}
finally
{
    await ftpFs.DisconnectAsync();
}
```

## 取消操作支持

所有文件系统操作都支持 `CancellationToken`，实现优雅的取消机制：

```csharp
public class FileUploadService
{
    private readonly IFileSystemOperations _fileSystem;
    
    public FileUploadService(IFileSystemOperations fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    // 带超时的上传
    public async Task<FileOperationResult> UploadWithTimeoutAsync(
        Stream stream, 
        string destinationPath, 
        int timeoutSeconds = 300)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        try
        {
            return await _fileSystem.UploadAsync(
                stream, 
                destinationPath, 
                overwrite: true, 
                cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            return FileOperationResult.Failure("上传因超时而取消");
        }
    }
    
    // 支持取消的批量上传
    public async Task<List<FileOperationResult>> UploadMultipleFilesAsync(
        Dictionary<Stream, string> files, 
        CancellationToken cancellationToken)
    {
        var results = new List<FileOperationResult>();
        
        foreach (var (stream, path) in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await _fileSystem.UploadAsync(
                stream, 
                path, 
                overwrite: true, 
                cancellationToken);
            results.Add(result);
        }
        
        return results;
    }
}
```

### 在 ASP.NET Core 中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileSystemOperations _fileSystem;
    
    public FileController(IFileSystemOperations fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        IFormFile file, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileSystem.UploadAsync(
                stream, 
                $"uploads/{file.FileName}", 
                overwrite: true, 
                cancellationToken);
            
            if (result.Success)
            {
                return Ok(new { path = result.FilePath });
            }
            
            return BadRequest(result.ErrorMessage);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "上传被客户端取消");
        }
    }
}
```

## 异常处理

```csharp
try
{
    var result = await ftpFs.UploadFileAsync("local.txt", "/remote");
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

// 为远程文件系统配置重试选项
var ftpFs = new FtpFileSystem(remoteSetting, retryOptions);
```

## 文件命名规则

本地文件系统支持三种文件命名规则：

- **Normal**: 保持原始文件名
- **Md5**: 使用文件内容的MD5哈希值命名
- **Uuid**: 使用UUID生成唯一文件名

```csharp
// 使用MD5命名规则
var result = await localFs.UploadAsync(
    fileStream, 
    "source.txt", 
    "container", 
    "uploads", 
    NamingRule.Md5
);

// 使用UUID命名规则
result = await localFs.UploadAsync(
    fileStream, 
    "source.txt", 
    "container", 
    "uploads", 
    NamingRule.Uuid
);
```

## 高级优化建议

### 本地文件系统 - 流式上传优化（v1.0.0+）

本地文件系统现已使用流式优化技术，显著降低文件上传时的内存占用：

**优化亮点：**
- ✅ **IncrementalHash**：增量计算 MD5 哈希值，无需将整个文件加载到内存
- ✅ **ArrayPool<byte>**：重用缓冲区内存，减少 GC 压力
- ✅ **流式 I/O**：分块处理文件，内存占用恒定，与文件大小无关

**性能提升：**

| 文件大小 | 旧实现 | 新实现 | 内存减少 |
|---------|--------|--------|---------|
| 100MB | ~100MB | ~8-256KB（缓冲区大小） | 99.9%+ |
| 1GB | ~1GB | ~8-256KB（缓冲区大小） | 99.99%+ |
| 10GB | ❌ 内存溢出 | ~8-256KB（缓冲区大小） | ✅ 支持 |

**技术细节：**

```csharp
// 旧方式（内存密集型）
var memoryStream = new MemoryStream();
await inputStream.CopyToAsync(memoryStream);
byte[] fileBytes = memoryStream.ToArray(); // 整个文件在内存中！
string hash = CalculateMD5(fileBytes);

// 新方式（流式 + IncrementalHash）
using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
try
{
    int bytesRead;
    while ((bytesRead = await inputStream.ReadAsync(buffer)) > 0)
    {
        await outputStream.WriteAsync(buffer, 0, bytesRead);
        md5.AppendData(buffer, 0, bytesRead);  // 增量更新哈希
    }
    string hash = BitConverter.ToString(md5.GetHashAndReset())...;
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);  // 归还缓冲区到池
}
```

**优化适用场景：**

- ✅ MD5 命名规则：使用临时文件进行流式哈希计算
- ✅ UUID 命名规则：直接流式写入目标文件
- ✅ Normal 命名规则：直接流式写入目标文件

**缓冲区配置：**

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    UploadBufferSize = 262144,  // 256KB 缓冲区（默认：81920 = 80KB）
    // 更大的缓冲区可提升大文件性能
    // 但会增加每个并发上传的内存占用
};
```

### 缓冲区与内存管理

为提高大文件处理性能，可以考虑以下优化：

```csharp
// 配置更高效的缓冲区大小
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",          // 根目录路径
    DefaultNamingRule = NamingRule.Md5,        // 默认命名规则: Md5、Uuid、Normal
    DefaultOverwrite = false,                  // 是否默认覆盖同名文件
    DefaultUseSequencedName = true,            // 文件名冲突时是否使用序号命名
    ValidationLevel = FileValidationLevel.Full, // 验证级别: None(不验证)、SizeOnly(仅大小)、Full(完整)
    CleanupOnValidationFailure = true,         // 验证失败时是否清理文件
    UploadBufferSize = 262144,                 // 增加到256KB以提升大文件上传性能
    DownloadBufferSize = 262144,               // 增加到256KB以提升大文件下载性能
    RetryOptions = new RetryOptions 
    { 
        MaxRetryAttempts = 3, 
        DelayMilliseconds = 1000 
    }
};
```

### 批量操作优化

对于需要处理大量文件的场景，可以使用批处理API减少连接开销：

```csharp
// FTP系统批量操作示例 - 比单个操作更高效
string[] localFiles = Directory.GetFiles("local/directory", "*.txt");
await ftpFs.UploadFilesAsync(localFiles, "/remote/path");
```

## 贡献

欢迎提交Pull Request和Issue帮助我们改进这个库。

## 许可证

MIT
