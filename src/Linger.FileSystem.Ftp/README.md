# Linger.FileSystem.Ftp

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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
ftpSystem.Connect();

// Use the file system
if (ftpSystem.FileExists("/remote/path/file.txt"))
{
    // Download file
    var fileContent = ftpSystem.ReadAllText("/remote/path/file.txt");
    
    // Process file content
    Console.WriteLine(fileContent);
}

// Create directory if it doesn't exist
ftpSystem.CreateDirectoryIfNotExists("/remote/path/new-directory");

// Upload a file
ftpSystem.WriteAllText("/remote/path/new-file.txt", "Hello, World!");

// Disconnect when done
ftpSystem.Disconnect();
```

### Asynchronous Operations

The library also provides asynchronous methods for all operations:

```csharp
// Connect asynchronously
await ftpSystem.ConnectAsync();

// Check if file exists asynchronously
if (await ftpSystem.FileExistsAsync("/remote/path/file.txt"))
{
    // Download file asynchronously
    var fileContent = await ftpSystem.ReadAllTextAsync("/remote/path/file.txt");
    
    // Process file content
    Console.WriteLine(fileContent);
}

// Disconnect asynchronously when done
await ftpSystem.DisconnectAsync();
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

## Dependencies

- [FluentFTP](https://github.com/robinrodricks/FluentFTP): Modern FTP client library
- [Linger.FileSystem](https://github.com/yourusername/Linger/tree/main/src/Linger.FileSystem): Core abstraction library

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
