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
var settings = new FtpFileSystemOptions
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
    var downloadedBytes = await ftpSystem.GetFileSizeAsync("/remote/path/file.txt");
    Console.WriteLine($"Downloaded {downloadedBytes} bytes");
}

```

### FTP Client Encoding

`FtpFileSystemOptions.Encoding` configures the encoding used by the FTP client for protocol text and remote path names. It defaults to UTF-8 when omitted:

```csharp
var settings = new FtpFileSystemOptions
{
    Host = "ftp.example.com",
    UserName = "username",
    Password = "password",
    Encoding = System.Text.Encoding.UTF8
};
```

This option does not control file-content encoding. Pass the required encoding to `GetReaderAsync` or `GetWriterAsync` when reading or writing text files.

### File Upload Methods

```csharp
// Method 1: Upload from stream to complete file path
await using var stream = File.OpenRead("local.txt");
var result = await ftpSystem.UploadAsync(stream, "/remote/path/file.txt", overwrite: true);

// Method 2: Upload local file to complete remote path
result = await ftpSystem.UploadFileAsync("C:/local/file.txt", "/remote/path/file.txt", overwrite: true);
```

## Integration with Dependency Injection

```csharp
// In your startup class
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IFileSystemOperations>(provider => {
        var settings = new FtpFileSystemOptions
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

`FtpFileSystem` automatically connects on the first operation and keeps one client connection per instance. Do not
invoke operations concurrently on the same instance or mutate its working directory while another operation is running.

## Advanced Features

### Working Directory Management

```csharp
// Set working directory
await ftpSystem.SetWorkingDirectoryAsync("/public_html");
```

### Directory Listing and Manipulation

```csharp
// List files
var files = await ftpSystem.ListFilesAsync("/public_html");
foreach (var file in files)
{
    Console.WriteLine($"File: {file}");
}

// Create directory
await ftpSystem.CreateDirectoryIfNotExistsAsync("/public_html/uploads");

// Check if directory exists
bool exists = await ftpSystem.DirectoryExistsAsync("/public_html/uploads");
```

### Custom Connection Settings

```csharp
var settings = new FtpFileSystemOptions
{
    Host = "ftp.example.com",
    Port = 21,
    UserName = "username",
    Password = "password",
    ConnectionTimeout = 30000,           // 30 seconds connection timeout
    OperationTimeout = 120000,           // 2 minutes operation timeout
    Type = "FTP"
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
```

### File Information and Metadata

```csharp
// Get file size
long? fileSize = await ftpSystem.GetFileSizeAsync("/remote/file.txt");

// Get file last modified time
DateTime modTime = await ftpSystem.GetModifiedTimeAsync("/remote/file.txt");

// Check if file exists
bool exists = await ftpSystem.FileExistsAsync("/remote/file.txt");
```

### Connection Lifetime

```csharp
using (var ftpSystem = new FtpFileSystem(settings))
{
    // The first operation establishes the connection; disposal closes it.
    await ftpSystem.UploadFileAsync("local.txt", "/remote/path");
    await ftpSystem.DownloadFileAsync("/remote/file.txt", "downloaded.txt");
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
