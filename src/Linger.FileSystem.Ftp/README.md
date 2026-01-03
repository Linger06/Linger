# Linger.FileSystem.Ftp

## Overview

Linger.FileSystem.Ftp is an implementation of the Linger FileSystem abstraction that provides FTP file operations support. It uses the FluentFTP library to offer a robust and retry-capable FTP client for common file operations such as uploading, downloading, listing, and deleting files.

## Installation

```bash
dotnet add package Linger.FileSystem.Ftp
```

## Features

- File operations over FTP (upload, download, list, delete)
- Configurable retry policies for unstable networks
- Timeout configurations
- Seamless integration with other Linger.FileSystem components
- Supports multiple .NET frameworks (net9.0, net8.0, netstandard2.0)

## Basic Usage

### Creating an FTP File System Instance

```csharp
// Create settings for remote FTP system
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000, // 15 seconds
    OperationTimeout = 60000   // 60 seconds
};

// Configure retry options
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// Create FTP file system
using var ftpSystem = new FtpFileSystem(settings, retryOptions);

// Connect to the server
await ftpSystem.ConnectAsync();

// Upload a file
await using var stream = File.OpenRead("./local/file.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

if (result.Success)
{
    Console.WriteLine($"Upload successful: {result.FilePath}");
}

// Download a file
var downloadResult = await ftpSystem.DownloadFileAsync("/remote/path/file.txt", "C:/Downloads/file.txt");

if (downloadResult.Success)
{
    Console.WriteLine($"Downloaded {downloadResult.FileSize} bytes");
}

// Disconnect when done
await ftpSystem.DisconnectAsync();
```

### File Upload Methods

```csharp
// Method 1: Upload from stream to complete file path
await using var stream = File.OpenRead("local.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// Method 2: Upload local file to complete remote path
result = await ftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);

// Method 3: Upload with separate directory and filename (convenient for dynamic naming)
result = await ftpSystem.UploadFileAsync(
    "C:/local/file.txt",           // Local file path
    "/remote/directory",            // Remote directory
    "custom-name.txt",              // Custom filename
    overwrite: true
);
```

## Integration with Dependency Injection

```csharp
// In your startup class
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFileSystemOperations>(provider => {
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
}
```

## Advanced Features

### Working Directory Management

```csharp
// Set working directory
await ftpSystem.SetWorkingDirectoryAsync("/public_html");

// Get current working directory
var currentDir = await ftpSystem.GetWorkingDirectoryAsync();
Console.WriteLine($"Current directory: {currentDir}");
```

### Directory Listing and Manipulation

```csharp
// List directory contents
var files = await ftpSystem.ListDirectoryAsync("/public_html");
foreach (var file in files)
{
    Console.WriteLine($"File: {file.Name}, Size: {file.Size}, Modified: {file.Modified}");
}

// Create directory
await ftpSystem.CreateDirectoryAsync("/public_html/uploads");

// Check if directory exists
bool exists = await ftpSystem.DirectoryExistsAsync("/public_html/uploads");
```

### Batch File Operations

```csharp
// Batch upload to a remote directory
var localFiles = new[] { "C:/data/a.txt", "C:/data/b.txt", "C:/data/c.txt" };
var uploadResult = await ftpSystem.UploadFilesAsync(localFiles, "/remote/uploads", overwrite: true);
Console.WriteLine($"Uploaded: {uploadResult.SucceededFiles.Count}, Failed: {uploadResult.FailedFiles.Count}");

// Batch download into a local directory
var remoteFiles = new[] { "/remote/uploads/a.txt", "/remote/uploads/b.txt" };
var downloadResult = await ftpSystem.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true);
Console.WriteLine($"Downloaded: {downloadResult.SucceededFiles.Count}, Failed: {downloadResult.FailedFiles.Count}");

// Batch delete
var deleteResult = await ftpSystem.DeleteFilesAsync(new[]
{
    "/remote/uploads/a.txt",
    "/remote/uploads/b.txt"
});
Console.WriteLine($"Deleted: {deleteResult.SucceededFiles.Count}, Failed: {deleteResult.FailedFiles.Count}");
```

Each batch call returns a `BatchOperationResult` containing `SucceededFiles` and `FailedFiles` with detailed error information.

### Batch Operation Progress Reporting

You can monitor batch operation progress using the `IProgress<BatchProgress>` parameter:

```csharp
// Create a progress handler
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"Progress: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"Current file: {p.CurrentFile}");
    Console.WriteLine($"Succeeded: {p.Succeeded}, Failed: {p.Failed}");
});

// Batch upload with progress reporting
var localFiles = new[] { "C:/data/a.txt", "C:/data/b.txt", "C:/data/c.txt" };
var result = await ftpSystem.UploadFilesAsync(localFiles, "/remote/uploads", overwrite: true, progress);

// Batch download with progress
var remoteFiles = new[] { "/remote/uploads/a.txt", "/remote/uploads/b.txt" };
var downloadResult = await ftpSystem.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// Batch delete with progress
var deleteResult = await ftpSystem.DeleteFilesAsync(new[] { "/remote/old.txt" }, progress);
```

The `BatchProgress` struct provides:
- `Completed`: Number of files processed (reported after each file completes)
- `Total`: Total number of files
- `CurrentFile`: Path of the file that was just processed
- `Succeeded`: Number of successful operations
- `Failed`: Number of failed operations
- `PercentComplete`: Completion percentage (0-100)

**Note**: Progress is reported *after* each file operation completes, ensuring `Completed` always reflects the accurate count.

### Custom Connection Settings

```csharp
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 30000,           // 30 seconds connection timeout
    OperationTimeout = 120000,           // 2 minutes operation timeout
    Type = "FTP",
    
    // Concurrency for batch operations: 1 = serial, >1 = parallel
    MaxDegreeOfParallelism = 4,
    
    // Connection pool idle timeout (optional)
    // Connections idle longer than this will be discarded and recreated
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // Batch operation retry settings
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    }
};

// Advanced retry configuration
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,
    DelayMilliseconds = 2000,
    MaxDelayMilliseconds = 30000,
    UseExponentialBackoff = true    // Use exponential backoff for retries
};

var ftpSystem = new FtpFileSystem(settings, retryOptions);

### Concurrency for Batch Operations

You can control parallelism for batch upload/download/delete via `RemoteSystemSetting.MaxDegreeOfParallelism`.

Behavior:
- `MaxDegreeOfParallelism = 1`: single connection, serial execution.
- `MaxDegreeOfParallelism > 1`: per-task independent `AsyncFtpClient` connections for thread safety and throughput.

Example:

```csharp
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000,
    OperationTimeout = 60000,
    MaxDegreeOfParallelism = 4
};

var ftp = new FtpFileSystem(settings);
await ftp.ConnectAsync();

var files = new[] { "C:/data/a.txt", "C:/data/b.txt", "C:/data/c.txt" };
var result = await ftp.UploadFilesAsync(files, "/remote/uploads", overwrite: true);
Console.WriteLine($"Uploaded: {result.SucceededFiles.Count}, Failed: {result.FailedFiles.Count}");
```
```

### File Information and Metadata

```csharp
// Get file size
long fileSize = await ftpSystem.GetFileSizeAsync("/remote/file.txt");

// Get file last modified time
DateTime modTime = await ftpSystem.GetModifiedTimeAsync("/remote/file.txt");

// Check if file exists
bool exists = await ftpSystem.FileExistsAsync("/remote/file.txt");
```

### Connection Management Best Practices

```csharp
// Method 1: Automatic connection management with using statement
using (var ftpSystem = new FtpFileSystem(settings))
{
    // Connection is automatically established and closed
    await ftpSystem.UploadFileAsync("local.txt", "/remote/path");
    await ftpSystem.DownloadFileAsync("/remote/file.txt", "downloaded.txt");
}

// Method 2: Manual connection management for multiple operations
var ftpSystem = new FtpFileSystem(settings);
try
{
    await ftpSystem.ConnectAsync();
    
    // Perform multiple operations efficiently
    for (int i = 0; i < 10; i++)
    {
        await ftpSystem.UploadFileAsync($"file{i}.txt", $"/remote/file{i}.txt");
    }
}
finally
{
    await ftpSystem.DisconnectAsync();
}
```

## Error Handling and Troubleshooting

### Common FTP Exceptions

```csharp
try
{
    await ftpSystem.UploadFileAsync("local.txt", "/remote/path");
}
catch (FileSystemException ex)
{
    switch (ex.Operation)
    {
        case "Upload":
            Console.WriteLine($"Upload failed: {ex.Message}");
            break;
        case "Connect":
            Console.WriteLine($"Connection failed: {ex.Message}");
            break;
    }
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Operation timed out: {ex.Message}");
}
```

### Retry Configuration for Unstable Networks

```csharp
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 10,             // Retry up to 10 times
    DelayMilliseconds = 1000,       // Start with 1 second delay
    MaxDelayMilliseconds = 60000,   // Maximum 60 seconds delay
    UseExponentialBackoff = true    // Increase delay exponentially
};

var ftpSystem = new FtpFileSystem(settings, retryOptions);
```

## Dependencies

- [FluentFTP](https://github.com/robinrodricks/FluentFTP): Modern FTP client library
- [Linger.FileSystem](https://github.com/Linger06/Linger/tree/main/src/Linger.FileSystem): Core abstraction library

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
