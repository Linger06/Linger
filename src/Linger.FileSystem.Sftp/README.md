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

## Dependencies

- [SSH.NET](https://github.com/sshnet/SSH.NET): A .NET implementation of the SSH2 client protocol
- [Linger.FileSystem](https://github.com/yourusername/Linger/tree/main/src/Linger.FileSystem): Core abstraction library

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
