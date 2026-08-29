# Linger.FileSystem

> Breaking changes and 2.0 migration notes are documented in the [Linger migration guide](../Linger/MIGRATION.md).

A file-system abstraction library for local storage, FTP, and SFTP. Content transfers share one asynchronous contract, while local metadata uses synchronous APIs and remote metadata keeps asynchronous protocol APIs.

## Project Structure

The Linger.FileSystem solution includes the following NuGet packages:

- **Linger.FileSystem**: Core library providing shared transfer contracts, local synchronous capabilities, and the local file-system implementation
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

## Key Features

- **Capability Contracts**: `IFileTransfer` unifies true asynchronous transfers, while local synchronous and remote asynchronous APIs use dedicated interfaces
- **Multiple File System Support**: Including local file system, FTP, and SFTP
- **True Asynchronous I/O**: Content transfers remain asynchronous without wrapping synchronous file-system APIs in `Task.Run`
- **Automatic Retry**: Built-in retry mechanism with configurable retry count and delay for improved operation reliability
- **Connection Management**: Automatic connection on demand, with reuse until the file-system instance is disposed
- **Multiple Naming Rules**: Support for MD5, UUID, and normal naming rules
- **Streaming Upload Optimization**: Local file system uses `IncrementalHash` and `ArrayPool<byte>` for memory-efficient large file processing, supporting files of any size
- **Batch Operation Progress**: Real-time progress tracking via `IProgress<BatchProgress>` for batch upload, download, and delete operations

## Supported .NET Versions

- .NET Framework 4.6.2+
- .NET Standard 2.0+
- .NET 5+

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

### Batch Operations (Local)

Local batch operations run in input order. Upload and download retain true asynchronous file I/O, while deletion is synchronous because `File.Delete` has no asynchronous API. The batch APIs centralize progress reporting, cancellation, target-conflict validation, and success/failure aggregation.

Example:

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage"
};

var localFs = new LocalFileSystem(options);

// Batch upload into a directory under the root (copies into C:/Storage/uploads)
var uploadResult = await localFs.UploadFilesAsync(new[]
{
    "C:/in/a.txt",
    "C:/in/b.txt"
}, "uploads", overwrite: true);
Console.WriteLine($"Uploaded: {uploadResult.SucceededFiles.Count}, Failed: {uploadResult.FailedFiles.Count}");

// Batch download: copy from root-relative paths into a local directory
var downloadResult = await localFs.DownloadFilesAsync(new[]
{
    "uploads/a.txt",
    "uploads/b.txt"
}, "C:/downloads", overwrite: true);
Console.WriteLine($"Downloaded: {downloadResult.SucceededFiles.Count}, Failed: {downloadResult.FailedFiles.Count}");

// Batch delete: pass root-relative paths
var deleteResult = localFs.DeleteFiles(new[]
{
    "uploads/a.txt",
    "uploads/b.txt"
});
Console.WriteLine($"Deleted: {deleteResult.SucceededFiles.Count}, Failed: {deleteResult.FailedFiles.Count}");
```

These batch APIs return `BatchOperationResult` with `SucceededFiles` and `FailedFiles` (failed items include error message and exception).

### Batch Operation Progress Reporting

Use `IProgress<BatchProgress>` to monitor the progress of batch operations:

```csharp
// Create a progress handler
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"Progress: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"Current file: {p.CurrentFile}");
    Console.WriteLine($"Succeeded: {p.Succeeded}, Failed: {p.Failed}");
});

// Batch upload with progress
var uploadResult = await localFs.UploadFilesAsync(files, "/uploads", overwrite: true, progress);

// Batch download with progress
var downloadResult = await localFs.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// Batch delete with progress
var deleteResult = localFs.DeleteFiles(filesToDelete, progress);
```

`BatchProgress` structure contains:
- `Completed`: Number of files processed (reported after each file completes)
- `Total`: Total number of files
- `CurrentFile`: Path of the file that was just processed
- `Succeeded`: Number of successful operations
- `Failed`: Number of failed operations
- `PercentComplete`: Completion percentage (0-100)

**Note**: Progress is reported *after* each file operation completes, ensuring `Completed` always reflects the accurate count.

### Using FTP File System

For FTP file system operations, install the FTP package:

```
Install-Package Linger.FileSystem.Ftp
```

Basic usage example:

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

📖 **For detailed FTP documentation and advanced features, see: [Linger.FileSystem.Ftp README](../Linger.FileSystem.Ftp/README.md)**

### Using SFTP File System

For SFTP file system operations, install the SFTP package:

```
Install-Package Linger.FileSystem.Sftp
```

Basic usage example:

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

📖 **For detailed SFTP documentation and advanced features, see: [Linger.FileSystem.Sftp README](../Linger.FileSystem.Sftp/README.md)**

## Common Operations

### File Upload

```csharp
// Upload stream
using var stream = File.OpenRead("local-file.txt");
var result = await fileSystem.UploadAsync(stream, "uploads/destination-file.txt", true);

// Upload local file
result = await fileSystem.UploadFileAsync("local-file.txt", "uploads", true);

// Upload with cancellation token
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
result = await fileSystem.UploadAsync(
    stream, 
    "uploads/destination-file.txt", 
    true, 
    cts.Token);
```

`FileOperationResult` does not include file size. Use `GetFileSize` locally and `GetFileSizeAsync` remotely.

### File Download

```csharp
// Download to stream
using var outputStream = new MemoryStream();
var result = await fileSystem.DownloadToStreamAsync("uploads/file.txt", outputStream);

// Download to local file
result = await fileSystem.DownloadFileAsync("uploads/file.txt", "C:/Downloads/downloaded-file.txt", true);

// Download with cancellation token
using var cts = new CancellationTokenSource();
result = await fileSystem.DownloadFileAsync(
    "uploads/large-file.zip", 
    "C:/Downloads/large-file.zip", 
    true, 
    cts.Token);
```

### File Deletion

```csharp
var localResult = localFileSystem.Delete("uploads/file-to-delete.txt");
var remoteResult = await remoteFileSystem.DeleteAsync("uploads/file-to-delete.txt");
```

### Directory Operations

```csharp
// Local metadata APIs are synchronous
bool localExists = localFileSystem.DirectoryExists("uploads/images");
localFileSystem.CreateDirectoryIfNotExists("uploads/documents");

// Remote protocol APIs remain truly asynchronous
bool remoteExists = await remoteFileSystem.DirectoryExistsAsync("uploads/images");
await remoteFileSystem.CreateDirectoryIfNotExistsAsync("uploads/documents");

```

The local file system uses synchronous `ListFiles` and `ListDirectories` for directory enumeration; FTP/SFTP remote enumeration uses asynchronous `ListFilesAsync` and `ListDirectoriesAsync`.

### Stream Factory API

Opening a local stream is synchronous; content I/O on the returned stream remains truly asynchronous:

```csharp
await using var readStream = localFileSystem.OpenRead("data/large-file.bin");
await ProcessLargeFileAsync(readStream);

// Open file for writing
await using var writeStream = localFileSystem.OpenWrite("output/result.bin", overwrite: true);
await writeStream.WriteAsync(data, cancellationToken);

// Text file reading with StreamReader
using var reader = localFileSystem.GetReader("logs/app.log", Encoding.UTF8);
while (await reader.ReadLineAsync() is { } line)
{
    ProcessLine(line);
}

// Text file writing with StreamWriter
await using var writer = localFileSystem.GetWriter("output/report.csv", overwrite: true, Encoding.UTF8);
await writer.WriteLineAsync("Name,Value");
await writer.WriteLineAsync("Item1,100");
```

### Metadata Query API

```csharp
// Local queries are synchronous; remote queries require protocol I/O
var fileSize = localFileSystem.GetFileSize("uploads/document.pdf");
var remoteFileSize = await remoteFileSystem.GetFileSizeAsync("uploads/document.pdf", cancellationToken);
if (fileSize.HasValue)
{
    Console.WriteLine($"File size: {fileSize.Value} bytes");
}
else
{
    Console.WriteLine("File not found");
}

// Remote metadata and working-directory operations are available through IRemoteFileSystem
var lastModified = await remoteFileSystem.GetLastModifiedTimeAsync("uploads/document.pdf", cancellationToken);
await remoteFileSystem.SetWorkingDirectoryAsync("uploads", cancellationToken);
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
    ValidationLevel = FileValidationLevel.SizeOnly, // Validation level: None, SizeOnly, Full
    CleanupOnValidationFailure = true,         // Whether to cleanup files on validation failure
    UploadBufferSize = 81920,                  // Upload buffer size
    DownloadBufferSize = 81920,                // Download buffer size
    RetryOptions = new RetryOptions 
    { 
        MaxRetryAttempts = 3, 
        DelayMilliseconds = 1000 
    }
};

var localFs = new LocalFileSystem(options);
```

### Remote File System Options

Remote configuration and connection lifecycles belong to their implementation packages. See [Linger.FileSystem.Ftp](../Linger.FileSystem.Ftp/README.md) and [Linger.FileSystem.Sftp](../Linger.FileSystem.Sftp/README.md).

## Advanced Features

### Local File System Advanced Features

#### File Naming Rules

The local file system supports three file naming rules:

- **Normal**: Keep original file name
- **Md5**: Use MD5 hash of file content for naming, ideal for deduplication
- **Uuid**: Use UUID to generate unique file name, ideal for avoiding conflicts

#### Advanced Upload Example

```csharp
// Use advanced upload functionality (with naming rule)
var uploadedInfo = await localFs.UploadWithNamingAsync(
    stream,
    "source-file.txt",  // Source file name
    "container1",       // Container name
    "images",           // Target path
    NamingRule.Md5,     // Naming rule: Normal, Md5, Uuid
    false,              // Whether to overwrite
    true                // Whether to use sequence naming
);

var uploadedFileInfo = await localFs.UploadFileWithNamingAsync(
    "source-file.txt",
    "container1",
    "images",
    NamingRule.Uuid);

// Access uploaded information
Console.WriteLine($"File hash: {uploadedInfo.HashData}");
Console.WriteLine($"Relative path: {uploadedInfo.FilePath}");
Console.WriteLine($"Full path: {uploadedInfo.FullFilePath}");
```

## Cancellation Support

All file system operations support `CancellationToken` for graceful cancellation:

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

## Exception Handling

```csharp
try
{
    var result = await fileSystem.UploadFileAsync("local.txt", "uploads/remote.txt");
    if (result.Success)
    {
        Console.WriteLine($"Upload successful: {result.FilePath}");
    }
    else
    {
        Console.WriteLine($"Upload failed: {result.ErrorMessage}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
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
    MaxRetryAttempts = 5,                        // Maximum 5 retries
    DelayMilliseconds = 1000,                 // Initial delay 1 second
    MaxDelayMilliseconds = 30000,             // Maximum delay 30 seconds
    UseExponentialBackoff = true              // Use exponential backoff algorithm
};

var localFs = new LocalFileSystem("C:/Storage", retryOptions);
```

## Performance Optimization

### Buffer Size Tuning

Default buffer size is 81920 bytes (80KB). For large file processing scenarios, you can increase the buffer size to improve throughput:

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    UploadBufferSize = 262144,    // 256KB - suitable for large file uploads
    DownloadBufferSize = 262144   // 256KB - suitable for large file downloads
};
```

Larger buffers can improve large-file throughput but increase memory usage for the active copy operation.

### Batch Operation Optimization

For scenarios requiring processing of many files, use the batch APIs to centralize progress, cancellation, conflict validation, and result aggregation:

```csharp
string[] localFiles = Directory.GetFiles("local/directory", "*.txt");
await localFs.UploadFilesAsync(localFiles, "uploads");
```

## Architecture Design

### Core Interface Hierarchy

```
                    IFileTransfer
                   /             \
      ILocalFileSystem       IRemoteFileSystem
                                      ^
                         IAsyncRemoteFileSystem (FTP)
```

### Core Interfaces

- **IFileTransfer**: Defines true asynchronous content transfers shared by local and remote implementations
- **ILocalFileSystem**: Defines synchronous local metadata and stream factories plus asynchronous content transfers
- **IRemoteFileSystem**: Defines remote metadata, stream, directory, transfer, and synchronous disposal capabilities
- **IAsyncRemoteFileSystem**: Adds true asynchronous disposal to the remote contract; currently implemented by `FtpFileSystem`

### Implementation Class Hierarchy

```
                FileSystemBase
                /           \
   LocalFileSystem     RemoteFileSystemBase
                         /         \
               FtpFileSystem    SftpFileSystem
```

### Base Classes

- **FileSystemBase**: Shares retry, logging, and batch-result helpers without declaring a local or remote execution contract
- **RemoteFileSystemBase**: Abstract base class that centralizes synchronous disposal and connection-gate cleanup
- **LocalFileSystem**: Concrete implementation of local file system
- **FtpFileSystem**: FTP file system implementation based on FluentFTP library, implementing `IAsyncRemoteFileSystem`
- **SftpFileSystem**: SFTP file system implementation based on SSH.NET library, with synchronous disposal only

### Design Patterns

This library uses the following design patterns:

- **Capability Interfaces**: Local and remote implementations share transfers while expressing metadata execution accurately
- **Template Method**: Defines algorithm skeleton in FileSystemBase base class, with subclasses implementing specific steps
- **Adapter Pattern**: Adapts different file system APIs like FluentFTP and SSH.NET to a unified interface
- **Simple Factory**: The CreateClient() method within each file system class creates specific client instances
- **Command Pattern**: Encapsulates operation results through FileOperationResult for unified execution status handling

### Key Workflows

1. **File Upload Workflow**:
   - Client calls `IFileTransfer.UploadAsync` for true asynchronous content transfer
   - Executes different implementations based on actual file system type
   - Applies configured file naming rules and validation measures
   - Returns unified FileOperationResult

2. **Remote System Connection Management**:
   - Uses `EnsureConnectedAsync()` for automatic connection management
   - Automatic connection on demand; the connection remains open until the instance is disposed

## Contributing

We welcome Pull Requests and Issues to help us improve this library.

## License

MIT
