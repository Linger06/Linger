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
IFileSystem (🚫 已过时)         IAsyncFileSystem (✅ 推荐)
    │                              │
    └───────────────┬──────────────┘
                    │
          IFileSystemOperations
           /            \
ILocalFileSystem    IRemoteFileSystem
```

### 核心接口

- **IFileSystem**: ⚠️ 已过时 - 定义基本同步文件操作接口（建议迁移到 IAsyncFileSystem）
- **IAsyncFileSystem**: ✅ 推荐 - 定义基本异步文件操作接口
- **IFileSystemOperations**: 统一的文件系统操作接口，继承自上述两个接口
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
- **组合模式**: 通过接口组合(IFileSystem和IAsyncFileSystem)构建更复杂的IFileSystemOperations
- **代理模式**: 远程文件系统的连接管理使用ConnectionScope作为代理，控制资源访问

### 关键流程

1. **文件上传流程**:
   - 客户端调用IFileSystemOperations.UploadAsync
   - 根据实际文件系统类型执行不同实现 
   - 应用配置的文件命名规则和验证措施
   - 返回统一的FileOperationResult结果

2. **远程系统连接管理**:
   - 使用ConnectionScope确保连接的正确打开和关闭
   - 自动连接、操作、关闭的生命周期管理

## 主要特点

- **统一接口**: 通过 `IFileSystemOperations` 接口提供的一致API操作不同类型的文件系统
- **支持多种文件系统**: 包括本地文件系统、FTP和SFTP
- **异步操作**: 所有操作都支持异步方法，适用于现代应用程序开发
- **自动重试**: 内置重试机制，可以配置重试次数和延迟，提高操作可靠性
- **连接管理**: 自动处理远程文件系统的连接和断开
- **多种命名规则**: 支持MD5、UUID和普通命名规则

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
```

### 文件下载

```csharp
// 下载到流
using var outputStream = new MemoryStream();
var result = await fileSystem.DownloadToStreamAsync("uploads/file.txt", outputStream);

// 下载到本地文件
result = await fileSystem.DownloadFileAsync("uploads/file.txt", "C:/Downloads/downloaded-file.txt", true);
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
    ValidateFileIntegrity = true,              // 是否验证文件完整性
    ValidateFileMetadata = false,              // 是否验证文件元数据
    CleanupOnValidationFailure = true,         // 验证失败时是否清理文件
    UploadBufferSize = 81920,                  // 上传缓冲区大小
    DownloadBufferSize = 81920,                // 下载缓冲区大小
    RetryOptions = new RetryOptions 
    { 
        MaxRetryCount = 3, 
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
    MaxRetryCount = 5,                        // 最多重试5次
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
    ValidateFileIntegrity = true,              // 是否验证文件完整性
    ValidateFileMetadata = false,              // 是否验证文件元数据
    CleanupOnValidationFailure = true,         // 验证失败时是否清理文件
    UploadBufferSize = 262144,                 // 增加到256KB以提升大文件上传性能
    DownloadBufferSize = 262144,               // 增加到256KB以提升大文件下载性能
    RetryOptions = new RetryOptions 
    { 
        MaxRetryCount = 3, 
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

## 迁移指南

### 从同步方法迁移到异步方法

⚠️ `IFileSystem` 接口中的同步方法已被标记为过时，建议迁移到异步版本以避免潜在的死锁和性能问题。

#### 为什么要迁移？

同步方法在现代应用程序中可能导致：
- **死锁风险**: 在 ASP.NET Core 等异步上下文中调用同步方法可能导致死锁
- **性能问题**: 阻塞线程池线程，降低应用程序的整体吞吐量
- **可扩展性限制**: 无法充分利用异步 I/O 的优势

#### 迁移步骤

**步骤 1: 更改接口类型**

```csharp
// ❌ 旧代码（使用同步接口）
IFileSystem fs = new LocalFileSystem("C:/Storage");

// ✅ 新代码 - 使用异步接口
IAsyncFileSystem fs = new LocalFileSystem("C:/Storage");

// ✅ 或使用完整的操作接口（推荐）
IFileSystemOperations fs = new LocalFileSystem("C:/Storage");
```

**步骤 2: 更新方法调用**

```csharp
// ❌ 旧代码（同步调用）
bool exists = fs.FileExists("/file.txt");
fs.CreateDirectoryIfNotExists("/uploads");
fs.DeleteFileIfExists("/temp.txt");

// ✅ 新代码（异步调用）
bool exists = await fs.FileExistsAsync("/file.txt");
await fs.CreateDirectoryIfNotExistsAsync("/uploads");
await fs.DeleteFileIfExistsAsync("/temp.txt");
```

**步骤 3: 更新方法签名为异步**

```csharp
// ❌ 旧方法签名（同步）
public void ProcessFile(string filePath)
{
    if (fs.FileExists(filePath))
    {
        // 处理文件...
        fs.DeleteFileIfExists(filePath);
    }
}

// ✅ 新方法签名（异步）
public async Task ProcessFileAsync(string filePath)
{
    if (await fs.FileExistsAsync(filePath))
    {
        // 处理文件...
        await fs.DeleteFileIfExistsAsync(filePath);
    }
}
```

**步骤 4: 更新调用链**

```csharp
// ❌ 旧代码
public class FileProcessor
{
    public void Run()
    {
        ProcessFile("data.txt");
    }
}

// ✅ 新代码
public class FileProcessor
{
    public async Task RunAsync()
    {
        await ProcessFileAsync("data.txt");
    }
}
```

#### 常见场景迁移示例

**场景 1: 文件上传前检查**

```csharp
// ❌ 旧代码
public FileOperationResult UploadFile(Stream stream, string path)
{
    if (fs.FileExists(path))
    {
        return FileOperationResult.Fail("文件已存在");
    }
    return fs.UploadAsync(stream, path).GetAwaiter().GetResult(); // 危险！
}

// ✅ 新代码
public async Task<FileOperationResult> UploadFileAsync(Stream stream, string path)
{
    if (await fs.FileExistsAsync(path))
    {
        return FileOperationResult.Fail("文件已存在");
    }
    return await fs.UploadAsync(stream, path);
}
```

**场景 2: 批量文件处理**

```csharp
// ❌ 旧代码
public void ProcessFiles(string[] filePaths)
{
    foreach (var path in filePaths)
    {
        if (fs.FileExists(path))
        {
            // 处理文件...
        }
    }
}

// ✅ 新代码
public async Task ProcessFilesAsync(string[] filePaths)
{
    foreach (var path in filePaths)
    {
        if (await fs.FileExistsAsync(path))
        {
            // 处理文件...
        }
    }
}

// ✅✅ 更好的方式 - 并行处理
public async Task ProcessFilesAsync(string[] filePaths)
{
    var tasks = filePaths.Select(async path =>
    {
        if (await fs.FileExistsAsync(path))
        {
            // 处理文件...
        }
    });
    await Task.WhenAll(tasks);
}
```

**场景 3: ASP.NET Core 控制器**

```csharp
// ❌ 旧代码（可能导致死锁！）
[HttpGet]
public IActionResult GetFile(string path)
{
    if (!fs.FileExists(path))  // 同步调用可能死锁
    {
        return NotFound();
    }
    // ...
}

// ✅ 新代码
[HttpGet]
public async Task<IActionResult> GetFileAsync(string path)
{
    if (!await fs.FileExistsAsync(path))
    {
        return NotFound();
    }
    // ...
}
```

#### 迁移检查清单

- [ ] 将所有 `IFileSystem` 类型声明改为 `IAsyncFileSystem` 或 `IFileSystemOperations`
- [ ] 将所有同步方法调用改为异步版本（添加 `Async` 后缀和 `await`）
- [ ] 将方法签名改为返回 `Task` 或 `Task<T>`
- [ ] 移除所有 `.GetAwaiter().GetResult()` 或 `.Wait()` 调用
- [ ] 更新单元测试为异步测试方法
- [ ] 在 ASP.NET Core 中确保控制器方法为异步

#### 向后兼容性

如果您的代码库需要同时支持旧代码，可以保留同步方法但添加编译警告抑制：

```csharp
#pragma warning disable CS0618 // 类型或成员已过时
IFileSystem fs = new LocalFileSystem("C:/Storage");
bool exists = fs.FileExists("/file.txt");
#pragma warning restore CS0618
```

但建议尽快完成迁移，因为同步方法可能在未来版本中被完全移除。

## 贡献

欢迎提交Pull Request和Issue帮助我们改进这个库。

## 许可证

MIT
