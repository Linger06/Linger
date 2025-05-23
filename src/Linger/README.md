# Linger.Utils

<a href="https://www.nuget.org/packages/Linger.Utils"><img src="https://img.shields.io/nuget/v/Linger.Utils.svg" alt="NuGet Version" /></a> 
<a href="https://www.nuget.org/packages/Linger.Utils"><img src="https://img.shields.io/nuget/dt/Linger.Utils.svg" alt="NuGet Download Count" /></a>

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

A comprehensive .NET utility library providing extensive type conversion extensions, string manipulation utilities, date/time helpers, file system operations, collection extensions, and various helper classes for everyday development tasks.

## Key Features

- **Type Conversion** - Robust conversion methods for all common data types
- **String Manipulation** - Advanced string processing and formatting utilities
- **DateTime Helpers** - Date and time calculation, formatting, and manipulation
- **File System Operations** - File and directory manipulation helpers
- **Collection Extensions** - Enhanced LINQ-style operations for collections
- **Cryptography Utilities** - Hashing, encryption and security helpers
- **Validation Helpers** - Data validation and verification utilities
- **JSON Utilities** - Simplified JSON serialization and deserialization

## Supported .NET versions

- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## Installation

### From Visual Studio

1. Open the `Solution Explorer`
2. Right-click on a project within your solution
3. Click on `Manage NuGet Packages...`
4. Click on the `Browse` tab and search for "Linger.Utils"
5. Click on the `Linger.Utils` package, select the appropriate version and click Install

### Package Manager Console

```
PM> Install-Package Linger.Utils
```

### .NET CLI Console

```
dotnet add package Linger.Utils
```

## Basic Usage Examples

### String Extensions

```csharp
// String conversions
int number = "123".ToInt();
decimal price = "45.67".ToDecimal();
bool isActive = "true".ToBool();
DateTime date = "2025-05-23".ToDateTime();

// String manipulations
string trimmed = " Hello World ".TrimBoth();
bool isEmail = "user@example.com".IsEmail();
bool containsLetters = "abc123".ContainsLetters();
```

### Collection Extensions

```csharp
// Collection operations
var list = new List<string> { "apple", "banana", "cherry" };
bool hasApple = list.ContainsIgnoreCase("APPLE");
var distinct = list.DistinctBy(item => item.Length);

// Safe operations
var firstItem = list.FirstOrDefault("default");
```

### File Operations

```csharp
// File operations
var file = new FileInfo("document.txt");
string text = file.ReadAllText();
byte[] bytes = file.ReadAllBytes();
```

## License

MIT License
