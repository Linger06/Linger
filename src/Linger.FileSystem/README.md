# Linger.FileSystem

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A unified file system abstraction library providing a consistent interface for accessing different file systems, including local file system, FTP, and SFTP. With this library, you can use the same API to operate on different types of file systems, simplifying the development process and improving code reusability.

## Project Structure

The Linger.FileSystem solution includes the following NuGet packages:

- **Linger.FileSystem**: Core library, providing unified interfaces and local file system implementation
- **Linger.FileSystem.Ftp**: FTP file system implementation, based on FluentFTP
- **Linger.FileSystem.Sftp**: SFTP file system implementation, based on SSH.NET

## Installation

```shell
# Install core library
dotnet add package Linger.FileSystem

# Install FTP support
dotnet add package Linger.FileSystem.Ftp

# Install SFTP support
dotnet add package Linger.FileSystem.Sftp
```

## Architecture Design

### Core Interface Hierarchy

```
IFileSystem                   IAsyncFileSystem
    │                              │
    └───────────────┬──────────────┘
                    │
          IFileSystemOperations
           /            \
ILocalFileSystem    IRemoteFileSystem
```

### Core Interfaces

- **IFileSystem**: Defines basic synchronous file operation interfaces
- **IAsyncFileSystem**: Defines basic asynchronous file operation interfaces
- **IFileSystemOperations**: Unified file system operation interface, inheriting from the above two interfaces
- **ILocalFileSystem**: Local file system specific interface, extending unique functionalities
- **IRemoteFileSystem**: Remote file system connection management interface

### Implementation Class Hierarchy

```
                FileSystemBase
                /           \
   LocalFileSystem     RemoteFileSystemBase
                         /         \
               FtpFileSystem    SftpFileSystem
```

### Base Classes

- **FileSystemBase**: Abstract base class for all file systems, implementing the IFileSystemOperations interface
- **RemoteFileSystemBase**: Abstract base class for remote file systems, inheriting from FileSystemBase and implementing IRemoteFileSystem
- **LocalFileSystem**: Concrete implementation of local file system
- **FtpFileSystem**: FTP file system implementation based on FluentFTP library
- **SftpFileSystem**: SFTP file system implementation based on SSH.NET library

### Design Patterns

This library uses the following design patterns:

- **Strategy Pattern**: Different file systems implement the same interface (IFileSystemOperations) but have their own implementation strategies
- **Template Method**: Defines algorithm skeleton in FileSystemBase base class, with subclasses implementing specific steps
- **Adapter Pattern**: Adapts different file system APIs like FluentFTP and SSH.NET to a unified interface
- **Simple Factory**: The CreateClient() method within each file system class creates specific client instances
- **Command Pattern**: Encapsulates operation results through FileOperationResult for unified execution status handling
- **Composite Pattern**: Builds more complex IFileSystemOperations through interface composition (IFileSystem and IAsyncFileSystem)
- **Proxy Pattern**: Remote file system connection management uses ConnectionScope as a proxy to control resource access

### Key Workflows

1. **File Upload Workflow**:
   - Client calls IFileSystemOperations.UploadAsync
   - Executes different implementations based on actual file system type
   - Applies configured file naming rules and validation measures
   - Returns unified FileOperationResult

2. **Remote System Connection Management**:
   - Uses ConnectionScope to ensure proper connection opening and closing
   - Automatic connection, operation, and disconnection lifecycle management

## Key Features

- **Unified Interface**: Consistent API through `IFileSystemOperations` interface to operate different types of file systems
- **Multiple File System Support**: Including local file system, FTP, and SFTP
- **Asynchronous Operations**: All operations support async methods, suitable for modern application development
- **Automatic Retry**: Built-in retry mechanism with configurable retry count and delay for improved operation reliability
- **Connection Management**: Automatic handling of remote file system connections and disconnections
- **Multiple Naming Rules**: Support for MD5, UUID, and normal naming rules

## Supported .NET Versions

- .NET Framework 4.6.2+
- .NET Standard 2.0+
- .NET Core 2.0+/.NET 5+

## Installation

Install via NuGet Package Manager:

```
Install-Package Linger.FileSystem
```

Or using .NET CLI:

```
dotnet add package Linger.FileSystem
```

## Quick Start

### Using Local File System

```csharp
// Create local file system instance
var localFs = new LocalFileSystem("C:/Storage");

// Upload file
using var fileStream = File.OpenRead("test.txt");
var result = await localFs.UploadAsync(fileStream, "uploads/destination-file.txt", true);

// Check upload result
if (result.Success)
{
    Console.WriteLine($"File uploaded successfully: {result.FilePath}");
}
```

### Using FTP File System

For FTP file system operations, install the FTP package:

```
Install-Package Linger.FileSystem.Ftp
```

Basic usage example:

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

📖 **For detailed FTP documentation and advanced features, see: [Linger.FileSystem.Ftp README](../Linger.FileSystem.Ftp/README.md)**

### Using SFTP File System

For SFTP file system operations, install the SFTP package:

```
Install-Package Linger.FileSystem.Sftp
```

Basic usage example:

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

📖 **For detailed SFTP documentation and advanced features, see: [Linger.FileSystem.Sftp README](../Linger.FileSystem.Sftp/README.md)**

## Common Operations

### File Upload

```csharp
// Upload stream
using var stream = File.OpenRead("local-file.txt");
var result = await fileSystem.UploadAsync(stream, "uploads/destination-file.txt", true);

// Upload local file
result = await fileSystem.UploadFileAsync("local-file.txt", "uploads", true);
```

### File Download

```csharp
// Download to stream
using var outputStream = new MemoryStream();
var result = await fileSystem.DownloadToStreamAsync("uploads/file.txt", outputStream);

// Download to local file
result = await fileSystem.DownloadFileAsync("uploads/file.txt", "C:/Downloads/downloaded-file.txt", true);
```

### File Deletion

```csharp
var result = await fileSystem.DeleteAsync("uploads/file-to-delete.txt");
```

### Directory Operations

```csharp
// Check if directory exists
bool exists = await fileSystem.DirectoryExistsAsync("uploads/images");

// Create directory
await fileSystem.CreateDirectoryIfNotExistsAsync("uploads/documents");
```

## Configuration Options

### Local File System Options

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",          // Root directory path
    DefaultNamingRule = NamingRule.Md5,        // Default naming rule: Md5, Uuid, Normal
    DefaultOverwrite = false,                  // Whether to overwrite files with same name by default
    DefaultUseSequencedName = true,            // Whether to use sequence naming on file name conflicts
    ValidateFileIntegrity = true,              // Whether to validate file integrity
    ValidateFileMetadata = false,              // Whether to validate file metadata
    CleanupOnValidationFailure = true,         // Whether to cleanup files on validation failure
    UploadBufferSize = 81920,                  // Upload buffer size
    DownloadBufferSize = 81920,                // Download buffer size
    RetryOptions = new RetryOptions 
    { 
        MaxRetryCount = 3, 
        DelayMilliseconds = 1000 
    }
};

var localFs = new LocalFileSystem(options);
```

### Remote File System Settings

```csharp
var remoteSetting = new RemoteSystemSetting
{
    Host = "example.com",                      // Host address
    Port = 21,                                 // Port (FTP default 21, SFTP default 22)
    UserName = "username",                     // Username
    Password = "password",                     // Password
    Type = "FTP",                              // Type: "FTP" or "SFTP"
    ConnectionTimeout = 30000,                 // Connection timeout (milliseconds)
    OperationTimeout = 60000,                  // Operation timeout (milliseconds)
    // SFTP specific settings
    CertificatePath = "",                      // Certificate path
    CertificatePassphrase = ""                 // Certificate passphrase
};
```

## Advanced Features

### Local File System Advanced Features

```csharp
// Use advanced upload functionality
var uploadedInfo = await localFs.UploadAsync(
    stream,
    "source-file.txt",  // Source file name
    "container1",       // Container name
    "images",           // Target path
    NamingRule.Md5,     // Naming rule
    false,              // Whether to overwrite
    true                // Whether to use sequence naming
);

// Access uploaded information
Console.WriteLine($"File hash: {uploadedInfo.HashData}");
Console.WriteLine($"Relative path: {uploadedInfo.FilePath}");
Console.WriteLine($"Full path: {uploadedInfo.FullFilePath}");
```

### FTP and SFTP Advanced Features

For advanced FTP and SFTP features such as:
- Batch file operations
- Directory listing and manipulation
- Working directory management
- Certificate-based authentication (SFTP)
- Custom timeout configurations

Please refer to the dedicated documentation:
- 📖 **[Linger.FileSystem.Ftp Documentation](../Linger.FileSystem.Ftp/README.md)**
- 📖 **[Linger.FileSystem.Sftp Documentation](../Linger.FileSystem.Sftp/README.md)**

## Connection Management

```csharp
// Method 1: Use using statement for automatic connection management
using (var ftpFs = new FtpFileSystem(remoteSetting))
{
    // Operations automatically handle connection and disconnection
    await ftpFs.UploadFileAsync("local.txt", "/remote/path");
}

// Method 2: Manual connection management
try
{
    ftpFs.Connect();
    // Execute multiple operations...
    await ftpFs.UploadFileAsync("file1.txt", "/remote");
    await ftpFs.UploadFileAsync("file2.txt", "/remote");
}
finally
{
    ftpFs.Disconnect();
}
```

## Exception Handling

```csharp
try
{
    var result = await ftpFs.UploadFileAsync("local.txt", "/remote");
    if (result.Success)
    {
        Console.WriteLine($"Upload successful: {result.FilePath}");
    }
    else
    {
        Console.WriteLine($"Upload failed: {result.ErrorMessage}");
    }
}
catch (FileSystemException ex)
{
    Console.WriteLine($"File system operation exception: {ex.Message}");
    Console.WriteLine($"Operation: {ex.Operation}, Path: {ex.Path}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}
```

## Retry Mechanism

```csharp
var retryOptions = new RetryOptions
{
    MaxRetryCount = 5,                        // Maximum 5 retries
    DelayMilliseconds = 1000,                 // Initial delay 1 second
    MaxDelayMilliseconds = 30000,             // Maximum delay 30 seconds
    UseExponentialBackoff = true              // Use exponential backoff algorithm
};

// Configure retry options for remote file system
var ftpFs = new FtpFileSystem(remoteSetting, retryOptions);
```

## File Naming Rules

The local file system supports three file naming rules:

- **Normal**: Keep original file name
- **Md5**: Use MD5 hash of file content for naming
- **Uuid**: Use UUID to generate unique file name

```csharp
// Use MD5 naming rule
var result = await localFs.UploadAsync(
    fileStream, 
    "source.txt", 
    "container", 
    "uploads", 
    NamingRule.Md5
);

// Use UUID naming rule
result = await localFs.UploadAsync(
    fileStream, 
    "source.txt", 
    "container", 
    "uploads", 
    NamingRule.Uuid
);
```

## Advanced Optimization Recommendations

### Buffer and Memory Management

To improve large file processing performance, consider the following optimizations:

```csharp
// Configure more efficient buffer size
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",          // Root directory path
    DefaultNamingRule = NamingRule.Md5,        // Default naming rule: Md5, Uuid, Normal
    DefaultOverwrite = false,                  // Whether to overwrite files with same name by default
    DefaultUseSequencedName = true,            // Whether to use sequence naming on file name conflicts
    ValidateFileIntegrity = true,              // Whether to validate file integrity
    ValidateFileMetadata = false,              // Whether to validate file metadata
    CleanupOnValidationFailure = true,         // Whether to cleanup files on validation failure
    UploadBufferSize = 262144,                 // Increase to 256KB to improve large file upload performance
    DownloadBufferSize = 262144,               // Increase to 256KB to improve large file download performance
    RetryOptions = new RetryOptions 
    { 
        MaxRetryCount = 3, 
        DelayMilliseconds = 1000 
    }
};
```

### Batch Operation Optimization

For scenarios requiring processing of large numbers of files, use batch processing APIs to reduce connection overhead:

```csharp
// FTP system batch operation example - more efficient than individual operations
string[] localFiles = Directory.GetFiles("local/directory", "*.txt");
await ftpFs.UploadFilesAsync(localFiles, "/remote/path");
```

## Contributing

We welcome Pull Requests and Issues to help us improve this library.

## License

MIT