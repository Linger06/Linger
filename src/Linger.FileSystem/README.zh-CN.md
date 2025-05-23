# Linger.FileSystem

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

Linger.FileSystem 是一个统一的文件系统抽象库，提供了对多种文件系统的一致访问接口，包括本地文件系统、FTP和SFTP。通过这个库，您可以使用相同的API操作不同类型的文件系统，简化开发过程，提高代码复用性。

## 项目结构

Linger.FileSystem解决方案包含以下NuGet包：

- **Linger.FileSystem**: 核心库，提供统一接口和本地文件系统实现
- **Linger.FileSystem.Ftp**: FTP文件系统实现，基于FluentFTP
- **Linger.FileSystem.Sftp**: SFTP文件系统实现，基于SSH.NET

## 安装方式

```shell
# 安装核心库
dotnet add package Linger.FileSystem

# 安装FTP支持
dotnet add package Linger.FileSystem.Ftp

# 安装SFTP支持
dotnet add package Linger.FileSystem.Sftp
```

## 快速入门

### 配置和创建文件系统

#### 本地文件系统

```csharp
// 基本用法
var localFs = new LocalFileSystem();

// 使用选项配置
var options = new LocalFileSystemOptions
{
    BasePath = "C:\\AppData\\Files",
    CreateBasePathIfNotExists = true,
    DefaultEncoding = Encoding.UTF8
};
var localFsWithOptions = new LocalFileSystem(options);

// 依赖注入注册
services.AddLocalFileSystem(options => 
{
    options.BasePath = "C:\\AppData\\Files";
    options.CreateBasePathIfNotExists = true;
});
```

#### FTP文件系统

```csharp
// 安装 Linger.FileSystem.Ftp 后
var ftpOptions = new FtpSystemOptions
{
    Host = "ftp.example.com",
    Port = 21,
    Username = "username",
    Password = "password",
    UseSsl = true,
    BaseDirectory = "/public_html/uploads"
};
var ftpFs = new FtpFileSystem(ftpOptions);

// 依赖注入注册
services.AddFtpFileSystem(options => 
{
    options.Host = "ftp.example.com";
    options.Username = "username";
    options.Password = "password";
});
```

#### SFTP文件系统

```csharp
// 安装 Linger.FileSystem.Sftp 后
var sftpOptions = new SftpSystemOptions
{
    Host = "sftp.example.com",
    Port = 22,
    Username = "username",
    Password = "password",
    BaseDirectory = "/home/username/uploads"
};
var sftpFs = new SftpFileSystem(sftpOptions);

// 依赖注入注册
services.AddSftpFileSystem(options => 
{
    options.Host = "sftp.example.com";
    options.Username = "username";
    options.Password = "password";
    // 或使用密钥文件
    options.KeyFilePath = "path/to/private_key.ppk";
    options.KeyFilePassphrase = "passphrase";
});
```

### 通用文件操作

由于使用统一接口，无论操作哪种文件系统，代码都几乎相同：

```csharp
// IFileSystemOperations 可以是任何实现，如 LocalFileSystem, FtpFileSystem 或 SftpFileSystem
public class FileService(IFileSystemOperations fileSystem)
{
    // 上传文件
    public async Task<string> UploadFileAsync(IFormFile file, string directory)
    {
        // 确保目录存在
        await fileSystem.CreateDirectoryIfNotExistsAsync(directory);
        
        // 生成唯一文件名
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(directory, fileName);
        
        // 上传文件
        using var stream = file.OpenReadStream();
        var result = await fileSystem.UploadAsync(stream, filePath);
        
        if (result.Success)
            return filePath;
            
        throw new Exception($"文件上传失败: {result.ErrorMessage}");
    }
    
    // 下载文件
    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        if (!await fileSystem.FileExistsAsync(filePath))
            throw new FileNotFoundException("文件不存在", filePath);
            
        // 获取文件流
        var memoryStream = new MemoryStream();
        await fileSystem.DownloadAsync(filePath, memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
    
    // 列出目录内容
    public async Task<IEnumerable<string>> ListFilesAsync(string directory)
    {
        if (!await fileSystem.DirectoryExistsAsync(directory))
            throw new DirectoryNotFoundException($"目录不存在: {directory}");
            
        var entries = await fileSystem.ListDirectoryAsync(directory);
        return entries.Where(e => !e.IsDirectory).Select(e => e.FullPath);
    }
    
    // 删除文件
    public async Task DeleteFileAsync(string filePath)
    {
        await fileSystem.DeleteFileIfExistsAsync(filePath);
    }
}
```

## 高级功能

### 文件操作结果处理

```csharp
public async Task<bool> SafeUploadAsync(Stream content, string destination)
{
    var result = await _fileSystem.UploadAsync(content, destination);
    
    if (result.Success)
    {
        Console.WriteLine($"文件上传成功: {result.Path}");
        return true;
    }
    else
    {
        Console.WriteLine($"错误: {result.ErrorMessage}");
        Console.WriteLine($"错误码: {result.ErrorCode}");
        Console.WriteLine($"异常: {result.Exception?.Message}");
        return false;
    }
}
```

### 异步流操作

```csharp
// 处理大文件
public async Task ProcessLargeFileAsync(string filePath)
{
    // 创建流但暂不加载全部内容到内存
    await using var stream = await _fileSystem.OpenReadStreamAsync(filePath);
    
    using var reader = new StreamReader(stream);
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        // 逐行处理文件
        await ProcessLineAsync(line);
    }
}
```

### 自定义命名规则

```csharp
var options = new LocalFileSystemOptions
{
    BasePath = "C:\\AppFiles",
    NamingRule = new NamingRule
    {
        // 自动将文件名转为小写
        FileNameTransform = name => name.ToLowerInvariant(),
        
        // 自定义目录命名规则
        DirectoryNameTransform = name => name.Replace(" ", "-")
    }
};

var fs = new LocalFileSystem(options);
```

## 接口和扩展性

核心接口：

- **IFileSystem**: 基础同步文件操作
- **IAsyncFileSystem**: 异步文件系统操作
- **IFileSystemOperations**: 组合接口，包含同步和异步操作
- **IRemoteFileSystemContext**: 远程文件系统特有操作

您可以实现这些接口来支持其他文件系统类型。

## 异常处理

库提供特定的异常类型：

```csharp
try
{
    await fileSystem.UploadAsync(stream, "existing/file.txt", overwrite: false);
}
catch (DuplicateFileException ex)
{
    // 处理文件已存在的情况
}
catch (FileSystemException ex)
{
    // 处理通用文件系统异常
}
```

## 支持的框架

- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
