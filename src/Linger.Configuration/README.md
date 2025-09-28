# Linger.Configuration

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A lightweight configuration helper library for .NET applications.

## Overview

Linger.Configuration provides utilities and extensions to simplify configuration management in .NET applications. It offers a consistent approach to access and bind configuration settings from various sources, with a focus on strongly-typed configuration.

## Features

- Simple singleton-based configuration access
- Strong typing for configuration settings
- Extension methods for IConfiguration
- Support for JSON configuration files
- Compatible with dependency injection
- Cross-platform support

## Installation

```bash
dotnet add package Linger.Configuration
```

## Usage

### Basic Usage

```csharp
// Access configuration singleton
var config = AppConfig.Instance.Config;

// Get configuration values
string connectionString = config.GetConnectionString("DefaultConnection");
int timeoutSeconds = config.GetValue<int>("AppSettings:TimeoutSeconds");

// Bind to strongly-typed objects
var smtpSettings = config.GetSection("SmtpSettings").Get<SmtpSettings>();
```

### Strongly-typed Configuration

```csharp
public class AppSettings
{
    public string ApplicationName { get; set; }
    public int CacheTimeoutMinutes { get; set; }
    public bool EnableLogging { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; }
    public string LoggingConnection { get; set; }
}

// Bind configuration
var appSettings = config.Get<AppSettings>();
Console.WriteLine($"App Name: {appSettings.ApplicationName}");
Console.WriteLine($"Cache Timeout: {appSettings.CacheTimeoutMinutes} minutes");
```

### Using with Dependency Injection

```csharp
// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add configuration
    services.AddSingleton<IConfiguration>(AppConfig.Instance.Config);
    
    // Register strongly-typed options
    services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
    
    // Use options pattern
    services.AddTransient<IMyService, MyService>();
}

// In a service
public class MyService : IMyService
{
    private readonly AppSettings _settings;
    
    public MyService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }
    
    public void DoSomething()
    {
        // Use settings
        if (_settings.EnableLogging)
        {
            // Log something
        }
    }
}
```

### Helper Methods

```csharp
// Get a typed value with a default
int timeout = AppSettingsHelper.GetValue("TimeoutSeconds", 30);

// Get a connection string
string connStr = AppSettingsHelper.GetConnectionString("DefaultConnection");

// Get a complex configuration object
var emailSettings = AppSettingsHelper.GetSection<EmailSettings>("EmailConfiguration");
```

## Dependencies

- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Configuration.Json
- Linger (Core utilities)

## License

This project is licensed under the terms of the license provided with the Linger project.
