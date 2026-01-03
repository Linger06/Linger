# Linger.FileSystem.Sftp

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
- 统一的批量操作接口与并发控制（`MaxDegreeOfParallelism`）

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
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// 创建 SFTP 文件系统
using var sftpSystem = new SftpFileSystem(settings, retryOptions);

// 连接到服务器
await sftpSystem.ConnectAsync();

// 上传文件
await using var stream = File.OpenRead("./local/file.txt");
var result = await sftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

if (result.Success)
{
    Console.WriteLine($"上传成功: {result.FilePath}");
}

// 下载文件
var downloadResult = await sftpSystem.DownloadFileAsync("/remote/path/file.txt", "C:/Downloads/file.txt");

if (downloadResult.Success)
{
    Console.WriteLine($"已下载 {downloadResult.FileSize} 字节");
}

// 完成后断开连接
await sftpSystem.DisconnectAsync();
```

### 文件上传方法

```csharp
// 方法 1: 从流上传到完整文件路径
await using var stream = File.OpenRead("local.txt");
var result = await sftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// 方法 2: 上传本地文件到完整远程路径
result = await sftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);

// 方法 3: 分别指定目录和文件名上传（便于动态命名）
result = await sftpSystem.UploadFileAsync(
    "C:/local/file.txt",           // 本地文件路径
    "/remote/directory",            // 远程目录
    "custom-name.txt",              // 自定义文件名
    overwrite: true
);
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

## 高级功能

### 工作目录管理

```csharp
// 获取当前工作目录
var currentDir = sftpSystem.GetCurrentDirectory();
Console.WriteLine($"当前目录：{currentDir}");

// 更改工作目录
sftpSystem.ChangeDirectory("/home/user/documents");

// 获取详细的目录列表
var files = sftpSystem.GetFiles("/remote/path", "*", SearchOption.TopDirectoryOnly);
foreach (var file in files)
{
    Console.WriteLine($"文件：{file}");
}

// 获取目录
var directories = sftpSystem.GetDirectories("/remote/path");
foreach (var dir in directories)
{
    Console.WriteLine($"目录：{dir}");
}
```

### 批量操作

```csharp
// 上传多个文件
var localFiles = new[]
{
    @"C:\local\file1.txt",
    @"C:\local\file2.txt",
    @"C:\local\file3.txt"
};

// 使用统一的批量接口进行上传/下载/删除
// 建议结合并发开关提升吞吐量
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000,
    OperationTimeout = 60000,
    MaxDegreeOfParallelism = 4 // 1 串行，>1 并发（每任务独立连接）
};

var sftp = new SftpFileSystem(settings);
await sftp.ConnectAsync();

// 批量上传
var uploadResult = await sftp.UploadFilesAsync(new[]
{
    "C:/local/file1.txt",
    "C:/local/file2.txt"
}, "/remote/uploads", overwrite: true);
Console.WriteLine($"上传成功: {uploadResult.SucceededFiles.Count}, 失败: {uploadResult.FailedFiles.Count}");

// 批量下载
var downloadResult = await sftp.DownloadFilesAsync(new[]
{
    "/remote/uploads/file1.txt",
    "/remote/uploads/file2.txt"
}, "C:/downloads", overwrite: true);
Console.WriteLine($"下载成功: {downloadResult.SucceededFiles.Count}, 失败: {downloadResult.FailedFiles.Count}");

// 批量删除
var deleteResult = await sftp.DeleteFilesAsync(new[]
{
    "/remote/uploads/file1.txt",
    "/remote/uploads/file2.txt"
});
Console.WriteLine($"删除成功: {deleteResult.SucceededFiles.Count}, 失败: {deleteResult.FailedFiles.Count}");

await sftp.DisconnectAsync();

// 失败详情在 FailedFiles 中，包含文件路径、错误信息与异常对象

```

### 批量操作进度报告

使用 `IProgress<BatchProgress>` 参数监控批量操作进度：

```csharp
// 创建进度处理器
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"进度: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"当前文件: {p.CurrentFile}");
    Console.WriteLine($"成功: {p.Succeeded}, 失败: {p.Failed}");
});

// 带进度报告的批量上传
var result = await sftp.UploadFilesAsync(files, "/remote/uploads", overwrite: true, progress);

// 带进度报告的批量下载
var downloadResult = await sftp.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// 带进度报告的批量删除
var deleteResult = await sftp.DeleteFilesAsync(filesToDelete, progress);
```

`BatchProgress` 结构包含：
- `Completed`: 已处理的文件数（每个文件处理完成后报告）
- `Total`: 总文件数
- `CurrentFile`: 刚处理完成的文件路径
- `Succeeded`: 成功的操作数
- `Failed`: 失败的操作数
- `PercentComplete`: 完成百分比 (0-100)

**说明**: 进度报告在每个文件操作*完成后*发送，确保 `Completed` 始终反映准确的计数。

### 并发支持

通过设置 `RemoteSystemSetting.MaxDegreeOfParallelism` 控制批量操作并发度：

- 值为 1：使用单连接串行执行，资源占用低，适合小规模任务；
- 值大于 1：每个任务使用独立的 `SftpClient` 连接，避免线程安全问题并提高吞吐量。
```

### 自定义连接设置

```csharp
// 使用自定义配置的高级 SFTP 设置
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 2222, // 自定义端口
    UserName = "username",
    Password = "password",
    
    // 连接设置
    ConnectionTimeout = 30000,    // 30 秒
    OperationTimeout = 120000,    // 2 分钟
    
    // 批量操作并发度
    MaxDegreeOfParallelism = 4,
    
    // 连接池空闲超时（可选）
    // 空闲超过此时间的连接将被丢弃并重新创建
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // 批量操作重试设置
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    },
    
    // 编码设置（如果需要特殊字符支持）
    Encoding = Encoding.UTF8
};

// 增强的重试配置
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,
    DelayMilliseconds = 2000,
    MaxDelayMilliseconds = 10000,
    UseExponentialBackoff = true // 指数退避
};

using var sftpSystem = new SftpFileSystem(settings, retryOptions);
```

### 错误处理和连接管理

```csharp
try
{
    sftpSystem.Connect();
    
    // 执行带自动重试的操作
    if (sftpSystem.FileExists("/remote/important-file.txt"))
    {
        var content = sftpSystem.ReadAllText("/remote/important-file.txt");
        
        // 安全地处理内容
        if (!string.IsNullOrEmpty(content))
        {
            // 保存备份
            sftpSystem.WriteAllText("/remote/backup/important-file.bak", content);
        }
    }
}
catch (SftpException ex)
{
    Console.WriteLine($"SFTP 错误：{ex.Message}");
    // 处理 SFTP 特定错误
}
catch (SshException ex)
{
    Console.WriteLine($"SSH 错误：{ex.Message}");
    // 处理 SSH 连接错误
}
catch (Exception ex)
{
    Console.WriteLine($"一般错误：{ex.Message}");
    // 处理其他错误
}
finally
{
    // 确保断开连接
    if (sftpSystem.IsConnected)
    {
        sftpSystem.Disconnect();
    }
}
```

### 文件权限和属性

```csharp
// 检查文件属性
var fileInfo = sftpSystem.GetFileInfo("/remote/path/file.txt");
Console.WriteLine($"文件大小：{fileInfo.Length} 字节");
Console.WriteLine($"最后修改时间：{fileInfo.LastWriteTime}");

// 创建具有特定权限的目录（类Unix系统）
sftpSystem.CreateDirectory("/remote/path/new-directory");

// 注意：文件权限通常在 SSH 服务器级别处理
// 检查文件是否可读/可写
if (sftpSystem.FileExists("/remote/path/file.txt"))
{
    try
    {
        var testContent = sftpSystem.ReadAllText("/remote/path/file.txt");
        Console.WriteLine("文件可读");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("文件不可读");
    }
}
```

### 大文件的流式操作

```csharp
// 流式下载大文件
using var remoteStream = sftpSystem.OpenRead("/remote/large-file.zip");
using var localStream = File.Create(@"C:\local\large-file.zip");

var buffer = new byte[8192]; // 8KB 缓冲区
int bytesRead;
long totalBytes = 0;

while ((bytesRead = await remoteStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
{
    await localStream.WriteAsync(buffer, 0, bytesRead);
    totalBytes += bytesRead;
    
    // 报告进度
    Console.WriteLine($"已下载：{totalBytes:N0} 字节");
}

Console.WriteLine($"下载完成：总共 {totalBytes:N0} 字节");
```

### 配置示例

#### 生产环境配置

```csharp
var productionSettings = new RemoteSystemSetting
{
    Host = "prod-sftp.company.com",
    Port = 22,
    UserName = "prod-user",
    CertificatePath = "/secure/certs/prod-key.pem",
    ConnectionTimeout = 15000,
    OperationTimeout = 300000, // 大文件需要 5 分钟
    Encoding = Encoding.UTF8
};

var productionRetry = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 5000,
    MaxDelayMilliseconds = 30000,
    UseExponentialBackoff = true
};
```

#### 开发环境配置

```csharp
var devSettings = new RemoteSystemSetting
{
    Host = "dev-sftp.company.com",
    Port = 22,
    UserName = "dev-user",
    Password = "dev-password",
    ConnectionTimeout = 10000,
    OperationTimeout = 60000,
    Encoding = Encoding.UTF8
};

var devRetry = new RetryOptions
{
    MaxRetryAttempts = 1,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};
```

## 高级功能

### 工作目录管理

```csharp
// 获取当前工作目录
var currentDir = sftpSystem.GetCurrentDirectory();
Console.WriteLine($"当前目录：{currentDir}");

// 更改工作目录
sftpSystem.ChangeDirectory("/home/user/documents");

// 获取详细的目录列表
var files = sftpSystem.GetFiles("/remote/path", "*", SearchOption.TopDirectoryOnly);
foreach (var file in files)
{
    Console.WriteLine($"文件：{file}");
}

// 获取目录
var directories = sftpSystem.GetDirectories("/remote/path");
foreach (var dir in directories)
{
    Console.WriteLine($"目录：{dir}");
}
```

### 批量操作

```csharp
// 上传多个文件
var localFiles = new[]
{
    @"C:\local\file1.txt",
    @"C:\local\file2.txt",
    @"C:\local\file3.txt"
};

foreach (var localFile in localFiles)
{
    var fileName = Path.GetFileName(localFile);
    var remotePath = $"/remote/uploads/{fileName}";
    
    // 带进度跟踪的上传
    await sftpSystem.CopyFileAsync(localFile, remotePath);
    Console.WriteLine($"已上传：{fileName}");
}

// 下载多个文件
var remoteFiles = sftpSystem.GetFiles("/remote/downloads", "*.txt");
foreach (var remoteFile in remoteFiles)
{
    var fileName = Path.GetFileName(remoteFile);
    var localPath = Path.Combine(@"C:\local\downloads", fileName);
    
    await sftpSystem.CopyFileAsync(remoteFile, localPath);
    Console.WriteLine($"已下载：{fileName}");
}
```

### 文件权限和属性

```csharp
// 检查文件属性
var fileInfo = sftpSystem.GetFileInfo("/remote/path/file.txt");
Console.WriteLine($"文件大小：{fileInfo.Length} 字节");
Console.WriteLine($"最后修改时间：{fileInfo.LastWriteTime}");

// 创建具有特定权限的目录（类Unix系统）
sftpSystem.CreateDirectory("/remote/path/new-directory");

// 注意：文件权限通常在SSH服务器层面处理
// 检查文件是否可读/可写
if (sftpSystem.FileExists("/remote/path/file.txt"))
{
    try
    {
        var testContent = sftpSystem.ReadAllText("/remote/path/file.txt");
        Console.WriteLine("文件可读");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("文件不可读");
    }
}
```

### 大文件的流式操作

```csharp
// 流式下载大文件
using var remoteStream = sftpSystem.OpenRead("/remote/large-file.zip");
using var localStream = File.Create(@"C:\local\large-file.zip");

var buffer = new byte[8192]; // 8KB 缓冲区
int bytesRead;
long totalBytes = 0;

while ((bytesRead = await remoteStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
{
    await localStream.WriteAsync(buffer, 0, bytesRead);
    totalBytes += bytesRead;
    
    // 报告进度
    Console.WriteLine($"已下载：{totalBytes:N0} 字节");
}

Console.WriteLine($"下载完成：总共 {totalBytes:N0} 字节");
```

### 配置示例

#### 生产环境配置

```csharp
var productionSettings = new RemoteSystemSetting
{
    Host = "prod-sftp.company.com",
    Port = 22,
    UserName = "prod-user",
    CertificatePath = "/secure/certs/prod-key.pem",
    ConnectionTimeout = 15000,
    OperationTimeout = 300000, // 大文件需要 5 分钟
    Encoding = Encoding.UTF8
};

var productionRetry = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 5000,
    MaxDelayMilliseconds = 30000,
    UseExponentialBackoff = true
};
```

#### 开发环境配置

```csharp
var devSettings = new RemoteSystemSetting
{
    Host = "dev-sftp.company.com",
    Port = 22,
    UserName = "dev-user",
    Password = "dev-password",
    ConnectionTimeout = 10000,
    OperationTimeout = 60000,
    Encoding = Encoding.UTF8
};

var devRetry = new RetryOptions
{
    MaxRetryAttempts = 1,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};
```

## 与依赖注入集成

```csharp
// 在您的启动类中
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFileSystemOperations>(provider => {
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
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1000,
            MaxDelayMilliseconds = 5000
        };
        
        return new SftpFileSystem(settings, retryOptions);
    });
}
```

## 最佳实践

1. **连接管理**：始终使用 `using` 语句或确保正确释放 SFTP 连接
2. **错误处理**：为 `SftpException` 和 `SshException` 实现特定的异常处理
3. **身份验证**：在生产环境中优先使用基于证书的身份验证而非密码
4. **超时设置**：根据网络条件和文件大小配置适当的超时
5. **重试逻辑**：使用指数退避进行重试尝试，避免压垮服务器
6. **大文件**：对于大于可用内存的文件使用流式操作
7. **安全性**：使用配置管理或密钥保管库安全地存储连接凭据

## 最佳实践

1. **连接管理**：始终使用 `using` 语句或确保正确释放 SFTP 连接
2. **错误处理**：为 `SftpException` 和 `SshException` 实现特定的异常处理
3. **认证方式**：在生产环境中优先使用基于证书的认证而非密码
4. **超时设置**：根据网络条件和文件大小配置适当的超时
5. **重试逻辑**：使用指数退避进行重试尝试，避免压垮服务器
6. **大文件**：对于大于可用内存的文件使用流式操作
7. **安全性**：使用配置管理或密钥保管库安全地存储连接凭据

## 依赖项

- [SSH.NET](https://github.com/sshnet/SSH.NET)
- [Linger.FileSystem](https://github.com/Linger06/Linger/tree/main/src/Linger.FileSystem)

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
