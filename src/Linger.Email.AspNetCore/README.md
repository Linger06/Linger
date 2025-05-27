# Linger.Email.AspNetCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.Email.AspNetCore provides ASP.NET Core integration for the Linger.Email library, making it easy to send emails in your ASP.NET Core applications. It includes dependency injection extensions and configuration integration to simplify email setup and management.

## Supported .NET versions

- .NET 9.0
- .NET 8.0

## Installation

```bash
dotnet add package Linger.Email.AspNetCore
```

## Features

- Simple integration with ASP.NET Core dependency injection
- Configuration binding from appsettings.json
- Support for multiple named email configurations
- Logging integration
- Fully compatible with Linger.Email features

## Basic Usage

### Configuration and Service Registration

1. Add email settings to your `appsettings.json`:

```json
{
  "EmailConfig": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "UserName": "username",
    "Password": "password",
    "From": {
      "Name": "My Application",
      "Address": "noreply@example.com"
    }
  }
}
```

2. Register services in your `Program.cs` or `Startup.cs`:

```csharp
using Linger.Email.AspNetCore;
using Linger.Email;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Use ConfigureEmail
builder.Services.ConfigureEmail(builder.Configuration);

// Option 2: Or use ConfigureMailKit
builder.Services.ConfigureMailKit(builder.Configuration);
```

### Using Email Service

```csharp
using Linger.Email;

public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailModel model)
    {
        var message = new EmailMessage
        {
            To = new List<EmailAddress> { new EmailAddress { Address = model.To } },
            Subject = model.Subject,
            Body = model.Body,
            IsHtmlBody = true
        };
        
        await _emailService.SendAsync(message);
        return Ok("Email sent successfully");
    }
}
```

## Advanced Features

### Multiple Email Configurations

You can configure different email configurations for different parts of your application:

```csharp
// In your startup
// Primary email service
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("PrimaryEmailConfig"));
services.AddTransient<IEmailService, EmailService>();

// Marketing email service
var marketingConfig = new EmailConfig
{
    Host = "smtp-marketing.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "marketing",
    Password = "password123",
    From = new EmailAddress { Name = "Marketing Team", Address = "marketing@example.com" }
};
builder.Services.AddSingleton<MarketingEmailService>(sp => new MarketingEmailService(marketingConfig));
```

Using named email senders:

```csharp
public class NotificationService
{
    private readonly IEmailService _emailService;
    private readonly MarketingEmailService _marketingEmailService;
    
    public NotificationService(
        IEmailService emailService, 
        MarketingEmailService marketingEmailService)
    {
        _emailService = emailService;
        _marketingEmailService = marketingEmailService;
    }
    
    public async Task SendRegularEmail(EmailMessage message)
    {
        await _emailService.SendAsync(message);
    }
    
    public async Task SendMarketingEmail(EmailMessage message)
    {
        await _marketingEmailService.SendAsync(message);
    }
}
```

## Dependencies

- [Linger.Email](../Linger.Email): Core email functionality
- [Linger.Configuration](../Linger.Configuration): Configuration abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.