# Linger.FileSystem

## Overview

Linger.FileSystem is a feature-rich C# file system helper library that provides a set of tools to simplify file and directory operations. The library aims to make developers more efficient in handling file system related tasks, including file upload and download.

## Table of Contents

- [Features](#features)
- [Supported .NET Versions](#supported-net-versions)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
- [Contributing](#contributing)
- [License](#license)

## Features

- Simplified file operation APIs
- Secure file read/write functionality
- Path processing utilities
- Cross-platform compatibility
- Asynchronous file operation support
- File upload and download capabilities

## Supported .NET Versions

This library supports the following .NET applications:
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

## Usage Examples

```csharp
using Linger.FileSystem;

// Basic file operations
var fileHelper = new FileHelper();
await fileHelper.WriteTextAsync("path/to/file.txt", "Hello World!");
var content = await fileHelper.ReadTextAsync("path/to/file.txt");

// Directory operations
var dirHelper = new DirectoryHelper();
dirHelper.CreateDirectory("path/to/directory");
var files = dirHelper.GetFiles("path/to/directory", "*.txt");

// LocalFileSystem operations
var localFS = new LocalFileSystem();
// Create and write to a file
await localFS.WriteAllTextAsync("local/path/to/file.txt", "Content to write");
// Check if file exists
bool exists = await localFS.FileExistsAsync("local/path/to/file.txt");
Console.WriteLine($"File exists: {exists}");
// Read file content
string fileContent = await localFS.ReadAllTextAsync("local/path/to/file.txt");
Console.WriteLine(fileContent);
// Copy file
await localFS.CopyAsync("source/file.txt", "destination/file.txt", true);
// Delete file
await localFS.DeleteFileAsync("local/path/to/file.txt");

// Upload and Download operations with LocalFileSystem
// Upload a file to a remote server
using (var fileStream = File.OpenRead("local/file/path.txt"))
{
    await localFS.UploadAsync(fileStream, "remote/destination/path.txt");
}

// Download a file from a remote server
await localFS.DownloadAsync("remote/source/file.txt", "local/destination/file.txt");

// Stream-based download
using (var destinationStream = File.Create("local/path/downloaded.file"))
{
    await localFS.DownloadToStreamAsync("remote/path/file", destinationStream);
}
```

## Contributing

Pull requests and issues are welcome to help us improve this library.

## License

MIT