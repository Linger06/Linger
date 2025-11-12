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
    MaxRetryCount = 3,
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
    services.AddSingleton<IFileSystem>(provider => {
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
            MaxRetryCount = 3,
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
// Batch upload files
string[] localFiles = { "file1.txt", "file2.txt", "file3.txt" };
int uploadedCount = await ftpSystem.UploadFilesAsync(localFiles, "/remote/uploads");
Console.WriteLine($"Uploaded {uploadedCount} files");

// Batch download files
string[] remoteFiles = { "/remote/file1.txt", "/remote/file2.txt" };
int downloadedCount = await ftpSystem.DownloadFilesAsync("C:/Downloads", remoteFiles);
Console.WriteLine($"Downloaded {downloadedCount} files");
```

### Custom Connection Settings

```csharp
var settings = new RemoteSystemSetting
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 30000,      // 30 seconds connection timeout
    OperationTimeout = 120000,      // 2 minutes operation timeout
    Type = "FTP"
};

// Advanced retry configuration
var retryOptions = new RetryOptions
{
    MaxRetryCount = 5,
    DelayMilliseconds = 2000,
    MaxDelayMilliseconds = 30000,
    UseExponentialBackoff = true    // Use exponential backoff for retries
};

var ftpSystem = new FtpFileSystem(settings, retryOptions);
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
    MaxRetryCount = 10,             // Retry up to 10 times
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
