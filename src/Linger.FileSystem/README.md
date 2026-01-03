# Linger.FileSystem

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
              IFileSystem
                   │
         IFileSystemOperations
           /            \
ILocalFileSystem    IRemoteFileSystem
```

### Core Interfaces

- **IFileSystem**: Defines basic file operation interfaces
- **IFileSystemOperations**: Unified file system operation interface, inheriting from IFileSystem
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

### Key Workflows

1. **File Upload Workflow**:
   - Client calls IFileSystemOperations.UploadAsync
   - Executes different implementations based on actual file system type
   - Applies configured file naming rules and validation measures
   - Returns unified FileOperationResult

2. **Remote System Connection Management**:
   - Uses `EnsureConnectedAsync()` for automatic connection management
   - Automatic connection, operation, and disconnection lifecycle management

## Key Features

- **Unified Interface**: Consistent API through `IFileSystemOperations` interface to operate different types of file systems
- **Multiple File System Support**: Including local file system, FTP, and SFTP
- **Asynchronous Operations**: All operations support async methods, suitable for modern application development
- **Automatic Retry**: Built-in retry mechanism with configurable retry count and delay for improved operation reliability
- **Connection Management**: Automatic handling of remote file system connections and disconnections
- **Multiple Naming Rules**: Support for MD5, UUID, and normal naming rules
- **Streaming Upload Optimization**: Local file system uses `IncrementalHash` and `ArrayPool<byte>` for memory-efficient large file processing (99.99% memory reduction)
- **Batch Operation Progress**: Real-time progress tracking via `IProgress<BatchProgress>` for batch upload, download, and delete operations
- **Connection Pool Idle Timeout**: Automatic cleanup of idle connections in the pool with configurable timeout via `ConnectionPoolIdleTimeout`
- **Batch Operation Retry**: Per-file retry support for batch operations with configurable `BatchRetryOptions` settings

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

### Concurrency and Batch (Local)

Local batch operations support configurable parallelism via `LocalFileSystemOptions.MaxDegreeOfParallelism`:

- `1`: serial execution (default), lower resource usage
- `>1`: parallel execution (internally throttled), ideal for large batches

Example:

```csharp
// Configure parallelism and use unified batch operations
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    MaxDegreeOfParallelism = 4 // 1 = serial, >1 = parallel
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
var deleteResult = await localFs.DeleteFilesAsync(new[]
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
var uploadResult = await fileSystem.UploadFilesAsync(files, "/uploads", overwrite: true, progress);

// Batch download with progress
var downloadResult = await fileSystem.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// Batch delete with progress
var deleteResult = await fileSystem.DeleteFilesAsync(filesToDelete, progress);
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

// Upload with cancellation token
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
result = await fileSystem.UploadAsync(
    stream, 
    "uploads/destination-file.txt", 
    true, 
    cts.Token);
```

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
var result = await fileSystem.DeleteAsync("uploads/file-to-delete.txt");
```

### Directory Operations

```csharp
// Check if directory exists
bool exists = await fileSystem.DirectoryExistsAsync("uploads/images");

// Create directory
await fileSystem.CreateDirectoryIfNotExistsAsync("uploads/documents");

// Check if path is a directory
if (await fileSystem.IsDirectoryAsync("uploads/images"))
{
    Console.WriteLine("Path is a directory");
}
```

### Stream Factory API

For efficient streaming operations without loading entire files into memory:

```csharp
// Open file for reading (returns raw Stream)
await using var readStream = await fileSystem.OpenReadAsync("data/large-file.bin", cancellationToken);
await ProcessLargeFileAsync(readStream);

// Open file for writing
await using var writeStream = await fileSystem.OpenWriteAsync("output/result.bin", overwrite: true, cancellationToken);
await writeStream.WriteAsync(data, cancellationToken);

// Text file reading with StreamReader
using var reader = await fileSystem.GetReaderAsync("logs/app.log", Encoding.UTF8, cancellationToken);
while (await reader.ReadLineAsync() is { } line)
{
    ProcessLine(line);
}

// Text file writing with StreamWriter
await using var writer = await fileSystem.GetWriterAsync("output/report.csv", overwrite: true, Encoding.UTF8, cancellationToken);
await writer.WriteLineAsync("Name,Value");
await writer.WriteLineAsync("Item1,100");
```

### Metadata Query API

```csharp
// Get file size (returns null if file doesn't exist)
var fileSize = await fileSystem.GetFileSizeAsync("uploads/document.pdf", cancellationToken);
if (fileSize.HasValue)
{
    Console.WriteLine($"File size: {fileSize.Value} bytes");
}
else
{
    Console.WriteLine("File not found");
}
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
    ValidationLevel = FileValidationLevel.Full, // Validation level: None, SizeOnly, Full
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
    MaxDegreeOfParallelism = 4,                // Batch operation concurrency
    
    // Connection pool idle timeout (connections idle longer than this will be recreated)
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // Batch operation retry settings
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    },
    
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

## Cancellation Support

All file system operations support `CancellationToken` for graceful cancellation:

```csharp
public class FileUploadService
{
    private readonly IFileSystemOperations _fileSystem;
    
    public FileUploadService(IFileSystemOperations fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    // Upload with timeout
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
            return FileOperationResult.Failure("Upload cancelled due to timeout");
        }
    }
    
    // Batch upload with cancellation
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

### Using with ASP.NET Core

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
            return StatusCode(499, "Upload cancelled by client");
        }
    }
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

### Local File System - Streaming Upload Optimization (v1.0.0+)

The local file system now uses streaming optimization to significantly reduce memory consumption during file uploads:

**Optimization Highlights:**
- ✅ **IncrementalHash**: Calculates MD5 hash incrementally instead of loading entire file into memory
- ✅ **ArrayPool<byte>**: Reuses buffer memory to reduce GC pressure
- ✅ **Streaming I/O**: Processes files in chunks, maintaining constant memory usage regardless of file size

**Performance Improvement:**

| File Size | Old Implementation | New Implementation | Memory Reduction |
|-----------|-------------------|-------------------|------------------|
| 100MB | ~100MB | ~8-256KB (buffer size) | 99.9%+ |
| 1GB | ~1GB | ~8-256KB (buffer size) | 99.99%+ |
| 10GB | ❌ Out of memory | ~8-256KB (buffer size) | ✅ Supported |

**Technical Details:**

```csharp
// Old approach (Memory-intensive)
var memoryStream = new MemoryStream();
await inputStream.CopyToAsync(memoryStream);
byte[] fileBytes = memoryStream.ToArray(); // Entire file in memory!
string hash = CalculateMD5(fileBytes);

// New approach (Streaming with IncrementalHash)
using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
try
{
    int bytesRead;
    while ((bytesRead = await inputStream.ReadAsync(buffer)) > 0)
    {
        await outputStream.WriteAsync(buffer, 0, bytesRead);
        md5.AppendData(buffer, 0, bytesRead);  // Incremental hash update
    }
    string hash = BitConverter.ToString(md5.GetHashAndReset())...;
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);  // Return buffer to pool
}
```

**When This Optimization Applies:**

- ✅ MD5 naming rule: Uses temporary file for streaming hash calculation
- ✅ UUID naming rule: Direct streaming to destination file
- ✅ Normal naming rule: Direct streaming to destination file

**Buffer Configuration:**

```csharp
var options = new LocalFileSystemOptions
{
    RootDirectoryPath = "C:/Storage",
    UploadBufferSize = 262144,  // 256KB buffer (default: 81920 = 80KB)
    // Larger buffers improve performance for large files
    // but increase memory per concurrent upload
};
```

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
    ValidationLevel = FileValidationLevel.Full, // Validation level: None, SizeOnly, Full
    CleanupOnValidationFailure = true,         // Whether to cleanup files on validation failure
    UploadBufferSize = 262144,                 // Increase to 256KB to improve large file upload performance
    DownloadBufferSize = 262144,               // Increase to 256KB to improve large file download performance
    RetryOptions = new RetryOptions 
    { 
        MaxRetryAttempts = 3, 
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