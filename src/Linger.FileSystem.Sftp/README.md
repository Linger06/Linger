# Linger.FileSystem.Sftp

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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
    MaxRetryCount = 3,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};

// Create SFTP file system
using var sftpSystem = new SftpFileSystem(settings, retryOptions);

// Connect to the server
sftpSystem.Connect();

// Use the file system
if (sftpSystem.FileExists("/remote/path/file.txt"))
{
    // Download file
    var fileContent = sftpSystem.ReadAllText("/remote/path/file.txt");
    
    // Process file content
    Console.WriteLine(fileContent);
}

// Create directory if it doesn't exist
sftpSystem.CreateDirectoryIfNotExists("/remote/path/new-directory");

// Upload a file
sftpSystem.WriteAllText("/remote/path/new-file.txt", "Hello, World!");

// Disconnect when done
sftpSystem.Disconnect();
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
// Upload multiple files
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
    
    // Upload with progress tracking
    await sftpSystem.CopyFileAsync(localFile, remotePath);
    Console.WriteLine($"Uploaded: {fileName}");
}

// Download multiple files
var remoteFiles = sftpSystem.GetFiles("/remote/downloads", "*.txt");
foreach (var remoteFile in remoteFiles)
{
    var fileName = Path.GetFileName(remoteFile);
    var localPath = Path.Combine(@"C:\local\downloads", fileName);
    
    await sftpSystem.CopyFileAsync(remoteFile, localPath);
    Console.WriteLine($"Downloaded: {fileName}");
}
```

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
    
    // Encoding settings (if needed for special characters)
    Encoding = Encoding.UTF8,
    
    // Buffer size for file operations (optional optimization)
    BufferSize = 32768  // 32KB buffer
};

// Enhanced retry configuration
var retryOptions = new RetryOptions
{
    MaxRetryCount = 5,
    DelayMilliseconds = 2000,
    MaxDelayMilliseconds = 10000,
    BackoffMultiplier = 2.0 // Exponential backoff
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
    MaxRetryCount = 3,
    DelayMilliseconds = 5000,
    MaxDelayMilliseconds = 30000,
    BackoffMultiplier = 2.0
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
    MaxRetryCount = 1,
    DelayMilliseconds = 1000,
    MaxDelayMilliseconds = 5000
};
```

## Integration with Dependency Injection

```csharp
// In your startup class
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFileSystem>(provider => {
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
            MaxRetryCount = 3,
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
