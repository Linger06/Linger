# Linger.FileSystem.Sftp

SFTP实现的文件系统操作库，提供简单易用的SFTP文件上传、下载和删除功能。

## 基本用法

```csharp
// 创建SFTP连接设置
var setting = RemoteSystemSetting.CreateSftp(
    "sftp.example.com", 
    22, 
    "username", 
    "password");

// 创建SFTP文件系统
using var sftp = new SftpRemoteFileSystem(setting);

// 上传文件
var result = await sftp.UploadFileAsync("local.txt", "remote/path/");

// 下载文件
var downloadResult = await sftp.DownloadFileAsync("remote/path/file.txt", "local/path/file.txt");

// 删除文件
var deleteResult = await sftp.DeleteAsync("remote/path/file.txt");
```

## 与IFileSystemOperations接口集成

该库实现了统一的`IFileSystemOperations`接口，可以与本地文件系统和FTP文件系统互换使用。

C# Helper Library

## Supported .NET versions

This library supports .NET applications that utilize .NET Framework 4.6.2+ or .NET Standard 2.0+.