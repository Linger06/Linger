# Linger.FileSystem.Sftp

## Overview

Linger.FileSystem.Sftp is an implementation of the Linger FileSystem abstraction that provides SFTP (SSH File Transfer Protocol) file operations support. It utilizes the SSH.NET library to offer a secure and reliable SFTP client for file operations with support for both password and certificate-based authentication.

## Installation

```bash
dotnet add package Linger.FileSystem.Sftp
```

## Features

- Secure file operations over SFTP (upload, download, list, delete)
- Support for both password and certificate-based authentication
- Configurable retry policies for unstable networks
- Timeout configurations
- Integration with the Linger.FileSystem abstraction
- Supports multiple .NET frameworks (net9.0, net8.0, netstandard2.0)
- Unified batch operations and concurrency control (`MaxDegreeOfParallelism`)

## Basic Usage

### Creating an SFTP File System Instance with Password Authentication

```csharp
// Create settings for remote SFTP system with password authentication
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
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

// Create SFTP file system
using var sftpSystem = new SftpFileSystem(settings, retryOptions);

// Connect to the server
await sftpSystem.ConnectAsync();

// Upload a file
await using var stream = File.OpenRead("./local/file.txt");
var result = await sftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

if (result.Success)
{
    Console.WriteLine($"Upload successful: {result.FilePath}");
}

// Download a file
var downloadResult = await sftpSystem.DownloadFileAsync("/remote/path/file.txt", "C:/Downloads/file.txt");

if (downloadResult.Success)
{
    Console.WriteLine($"Downloaded {downloadResult.FileSize} bytes");
}

// Disconnect when done
await sftpSystem.DisconnectAsync();
```

### File Upload Methods

```csharp
// Method 1: Upload from stream to complete file path
await using var stream = File.OpenRead("local.txt");
var result = await sftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// Method 2: Upload local file to complete remote path
result = await sftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);

// Method 3: Upload with separate directory and filename (convenient for dynamic naming)
result = await sftpSystem.UploadFileAsync(
    "C:/local/file.txt",           // Local file path
    "/remote/directory",            // Remote directory
    "custom-name.txt",              // Custom filename
    overwrite: true
);
```

### Using Certificate-based Authentication

```csharp
// Create settings for remote SFTP system with certificate authentication
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    CertificatePath = "/path/to/private/key.pem",
    CertificatePassphrase = "optional-passphrase", // If the private key is protected with a passphrase
    ConnectionTimeout = 15000, // 15 seconds
    OperationTimeout = 60000   // 60 seconds
};

// Create SFTP file system with certificate authentication
using var sftpSystem = new SftpFileSystem(settings);

// Connect and use as normal
sftpSystem.Connect();
// ... perform operations ...
sftpSystem.Disconnect();
```

### Asynchronous Operations

The library also provides asynchronous methods for all operations:

```csharp
// Connect asynchronously
await sftpSystem.ConnectAsync();

// Check if file exists asynchronously
if (await sftpSystem.FileExistsAsync("/remote/path/file.txt"))
{
    // Download file asynchronously
    var fileContent = await sftpSystem.ReadAllTextAsync("/remote/path/file.txt");
    
    // Process file content
    Console.WriteLine(fileContent);
}

// Disconnect asynchronously when done
await sftpSystem.DisconnectAsync();
```

## Advanced Features

### Working Directory Management

```csharp
// Get current working directory
var currentDir = sftpSystem.GetCurrentDirectory();
Console.WriteLine($"Current directory: {currentDir}");

// Change working directory
sftpSystem.ChangeDirectory("/home/user/documents");

// Get directory listing with details
var files = sftpSystem.GetFiles("/remote/path", "*", SearchOption.TopDirectoryOnly);
foreach (var file in files)
{
    Console.WriteLine($"File: {file}");
}

// Get directories
var directories = sftpSystem.GetDirectories("/remote/path");
foreach (var dir in directories)
{
    Console.WriteLine($"Directory: {dir}");
}
```

### Batch Operations

```csharp
// Use the unified batch operations interface
// Combine with concurrency to improve throughput
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 22,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 15000,
    OperationTimeout = 60000,
    MaxDegreeOfParallelism = 4 // 1 = serial, >1 = parallel (per-task connection)
};

var sftp = new SftpFileSystem(settings);
await sftp.ConnectAsync();

// Batch upload
var uploadResult = await sftp.UploadFilesAsync(new[]
{
    "C:/local/file1.txt",
    "C:/local/file2.txt"
}, "/remote/uploads", overwrite: true);
Console.WriteLine($"Uploaded: {uploadResult.SucceededFiles.Count}, Failed: {uploadResult.FailedFiles.Count}");

// Batch download
var downloadResult = await sftp.DownloadFilesAsync(new[]
{
    "/remote/uploads/file1.txt",
    "/remote/uploads/file2.txt"
}, "C:/downloads", overwrite: true);
Console.WriteLine($"Downloaded: {downloadResult.SucceededFiles.Count}, Failed: {downloadResult.FailedFiles.Count}");

// Batch delete
var deleteResult = await sftp.DeleteFilesAsync(new[]
{
    "/remote/uploads/file1.txt",
    "/remote/uploads/file2.txt"
});
Console.WriteLine($"Deleted: {deleteResult.SucceededFiles.Count}, Failed: {deleteResult.FailedFiles.Count}");

await sftp.DisconnectAsync();

// Failed items are available in FailedFiles with path, message, and exception
```

### Batch Operation Progress Reporting

Monitor batch operation progress using the `IProgress<BatchProgress>` parameter:

```csharp
// Create a progress handler
var progress = new Progress<BatchProgress>(p =>
{
    Console.WriteLine($"Progress: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)");
    Console.WriteLine($"Current file: {p.CurrentFile}");
    Console.WriteLine($"Succeeded: {p.Succeeded}, Failed: {p.Failed}");
});

// Batch upload with progress
var result = await sftp.UploadFilesAsync(files, "/remote/uploads", overwrite: true, progress);

// Batch download with progress
var downloadResult = await sftp.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true, progress);

// Batch delete with progress
var deleteResult = await sftp.DeleteFilesAsync(filesToDelete, progress);
```

The `BatchProgress` struct provides:
- `Completed`: Number of files processed (reported after each file completes)
- `Total`: Total number of files
- `CurrentFile`: Path of the file that was just processed
- `Succeeded`: Number of successful operations
- `Failed`: Number of failed operations  
- `PercentComplete`: Completion percentage (0-100)

**Note**: Progress is reported *after* each file operation completes, ensuring `Completed` always reflects the accurate count.

### Concurrency

Control parallelism for batch operations via `RemoteSystemSetting.MaxDegreeOfParallelism`:

- `1`: single connection, serial execution (lower resource usage).
- `>1`: independent `SftpClient` connection per task for thread safety and improved throughput.

### Custom Connection Settings

```csharp
// Advanced SFTP settings with custom configurations
var settings = new RemoteSystemSetting
{
    Host = "sftp.example.com",
    Port = 2222, // Custom port
    UserName = "username",
    Password = "password",
    
    // Connection settings
    ConnectionTimeout = 30000,    // 30 seconds
    OperationTimeout = 120000,    // 2 minutes
    
    // Concurrency for batch operations
    MaxDegreeOfParallelism = 4,
    
    // Connection pool idle timeout (optional)
    // Connections idle longer than this will be discarded and recreated
    ConnectionPoolIdleTimeout = TimeSpan.FromMinutes(5),
    
    // Batch operation retry settings
    BatchRetryOptions = new RetryOptions
    {
        MaxRetryAttempts = 3,
        DelayMilliseconds = 1000
    },
    
    // Encoding settings (if needed for special characters)
    Encoding = Encoding.UTF8
};

// Enhanced retry configuration
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,
    DelayMilliseconds = 2000,
    MaxDelayMilliseconds = 10000,
    UseExponentialBackoff = true // Exponential backoff
};

using var sftpSystem = new SftpFileSystem(settings, retryOptions);
```

### Error Handling and Connection Management

```csharp
try
{
    sftpSystem.Connect();
    
    // Perform operations with automatic retry
    if (sftpSystem.FileExists("/remote/important-file.txt"))
    {
        var content = sftpSystem.ReadAllText("/remote/important-file.txt");
        
        // Process content safely
        if (!string.IsNullOrEmpty(content))
        {
            // Save backup
            sftpSystem.WriteAllText("/remote/backup/important-file.bak", content);
        }
    }
}
catch (SftpException ex)
{
    Console.WriteLine($"SFTP Error: {ex.Message}");
    // Handle SFTP-specific errors
}
catch (SshException ex)
{
    Console.WriteLine($"SSH Error: {ex.Message}");
    // Handle SSH connection errors
}
catch (Exception ex)
{
    Console.WriteLine($"General Error: {ex.Message}");
    // Handle other errors
}
finally
{
    // Ensure disconnection
    if (sftpSystem.IsConnected)
    {
        sftpSystem.Disconnect();
    }
}
```

### File Permissions and Attributes

```csharp
// Check file attributes
var fileInfo = sftpSystem.GetFileInfo("/remote/path/file.txt");
Console.WriteLine($"File size: {fileInfo.Length} bytes");
Console.WriteLine($"Last modified: {fileInfo.LastWriteTime}");

// Create directory with specific permissions (Unix-like systems)
sftpSystem.CreateDirectory("/remote/path/new-directory");

// Note: File permissions are typically handled at the SSH server level
// Check if file is readable/writable
if (sftpSystem.FileExists("/remote/path/file.txt"))
{
    try
    {
        var testContent = sftpSystem.ReadAllText("/remote/path/file.txt");
        Console.WriteLine("File is readable");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("File is not readable");
    }
}
```

### Streaming Operations for Large Files

```csharp
// Stream large file download
using var remoteStream = sftpSystem.OpenRead("/remote/large-file.zip");
using var localStream = File.Create(@"C:\local\large-file.zip");

var buffer = new byte[8192]; // 8KB buffer
int bytesRead;
long totalBytes = 0;

while ((bytesRead = await remoteStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
{
    await localStream.WriteAsync(buffer, 0, bytesRead);
    totalBytes += bytesRead;
    
    // Report progress
    Console.WriteLine($"Downloaded: {totalBytes:N0} bytes");
}

Console.WriteLine($"Download completed: {totalBytes:N0} bytes total");
```

### Configuration Examples

#### Production Configuration

```csharp
var productionSettings = new RemoteSystemSetting
{
    Host = "prod-sftp.company.com",
    Port = 22,
    UserName = "prod-user",
    CertificatePath = "/secure/certs/prod-key.pem",
    ConnectionTimeout = 15000,
    OperationTimeout = 300000, // 5 minutes for large files
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

#### Development Configuration

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

## Integration with Dependency Injection

```csharp
// In your startup class
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

## Best Practices

1. **Connection Management**: Always use `using` statements or ensure proper disposal of SFTP connections
2. **Error Handling**: Implement specific exception handling for `SftpException` and `SshException`
3. **Authentication**: Prefer certificate-based authentication over passwords for production environments
4. **Timeouts**: Configure appropriate timeouts based on your network conditions and file sizes
5. **Retry Logic**: Use exponential backoff for retry attempts to avoid overwhelming the server
6. **Large Files**: Use streaming operations for files larger than available memory
7. **Security**: Store connection credentials securely using configuration management or key vaults

## Dependencies

- [SSH.NET](https://github.com/sshnet/SSH.NET)
- [Linger.FileSystem](https://github.com/Linger06/Linger/tree/main/src/Linger.FileSystem)

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
