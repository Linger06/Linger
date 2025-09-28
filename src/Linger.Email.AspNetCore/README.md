# Linger.Email.AspNetCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.Email.AspNetCore provides seamless ASP.NET Core integration for the Linger.Email library. It simplifies email service configuration through dependency injection, configuration binding, and provides enhanced email service capabilities specifically designed for modern ASP.NET Core applications.

## Installation

```bash
dotnet add package Linger.Email.AspNetCore
```

## Features

- **Dependency Injection Integration**: Simple service registration and configuration
- **Configuration Binding**: Automatic binding from appsettings.json
- **Enhanced Email Service**: Additional convenience methods for common email scenarios
- **Logging Integration**: Built-in logging support for email operations
- **Configuration Validation**: Automatic validation of email configuration
- **Cross-platform**: Supports .NET 9.0 and .NET 8.0

## Quick Start

### 1. Configure Email Settings

Add email configuration to your `appsettings.json`:

```json
{
  "EmailConfig": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": {
      "Address": "your-email@gmail.com",
      "Name": "Your Application"
    },
    "Bcc": [
      {
        "Address": "audit@yourcompany.com",
        "Name": "Audit Trail"
      }
    ]
  }
}
```

### 2. Register Email Services

In your `Program.cs` (or `Startup.cs` for older versions):

```csharp
using Linger.Email.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Method 1: Simple configuration binding
builder.Services.ConfigureEmail(builder.Configuration);

// Method 2: Using MailKit configuration (alternative)
// builder.Services.ConfigureMailKit(builder.Configuration);

var app = builder.Build();
```

### 3. Use Email Service in Controllers

```csharp
using Linger.Email.AspNetCore;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-text")]
    public async Task<IActionResult> SendTextEmail([FromBody] BasicEmailRequest request)
    {
        await _emailService.SendTextEmailAsync(
            request.To, 
            request.Subject, 
            request.Body
        );

        return Ok("Text email sent successfully");
    }

    [HttpPost("send-html")]
    public async Task<IActionResult> SendHtmlEmail([FromBody] BasicEmailRequest request)
    {
        await _emailService.SendHtmlEmailAsync(
            request.To, 
            request.Subject, 
            request.Body
        );

        return Ok("HTML email sent successfully");
    }

    [HttpPost("send-with-attachments")]
    public async Task<IActionResult> SendWithAttachments([FromBody] EmailWithAttachmentsRequest request)
    {
        await _emailService.SendWithAttachmentsAsync(
            request.To,
            request.Subject,
            request.Body,
            request.IsHtml,
            request.AttachmentPaths
        );

        return Ok("Email with attachments sent successfully");
    }
}

public record BasicEmailRequest(string To, string Subject, string Body);
public record EmailWithAttachmentsRequest(string To, string Subject, string Body, bool IsHtml, List<string> AttachmentPaths);
```

## Advanced Usage

### Using IEmailService Methods

The `IEmailService` provides convenient methods for common email scenarios:

```csharp
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // Send simple text email
    public async Task SendTextEmail()
    {
        await _emailService.SendTextEmailAsync(
            "user@example.com",
            "Plain Text Subject",
            "This is a plain text email body."
        );
    }

    // Send HTML email
    public async Task SendHtmlEmail()
    {
        await _emailService.SendHtmlEmailAsync(
            "user@example.com",
            "HTML Email Subject",
            "<h1>Hello!</h1><p>This is an HTML email.</p>"
        );
    }

    // Send email with attachments
    public async Task SendEmailWithAttachments()
    {
        var attachmentPaths = new[]
        {
            @"C:\temp\report.pdf",
            @"C:\temp\image.jpg"
        };

        await _emailService.SendWithAttachmentsAsync(
            "user@example.com",
            "Email with Attachments",
            "<p>Please find the attached files.</p>",
            isHtml: true,
            attachmentPaths
        );
    }
}
```

### Advanced Email Message Configuration

For more complex scenarios, use the underlying `SendAsync` method:

```csharp
public class AdvancedEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public AdvancedEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendAdvancedEmail()
    {
        var message = new EmailMessage
        {
            To = new List<EmailAddress>
            {
                new("primary@example.com", "Primary Recipient"),
                new("secondary@example.com", "Secondary Recipient")
            },
            Cc = new List<EmailAddress>
            {
                new("manager@example.com", "Manager")
            },
            Subject = "Project Status Update",
            Body = @"
                <h2>Project Status Report</h2>
                <p>Dear Team,</p>
                <p>Here's the latest status of our project:</p>
                <ul>
                    <li>✅ Phase 1: Completed</li>
                    <li>🔄 Phase 2: In Progress</li>
                    <li>⏳ Phase 3: Pending</li>
                </ul>
                <p>Best regards,<br>Project Manager</p>
            ",
            IsHtmlBody = true,
            Priority = MessagePriority.High
        };

        await _emailService.SendAsync(message, response =>
        {
            // Optional callback for handling send completion
            Console.WriteLine($"Email sent: {response}");
        });
    }
}
```

### Background Email Service

For sending emails in background tasks:

```csharp
public class BackgroundEmailService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundEmailService> _logger;

    // Important: Use IServiceScopeFactory instead of directly injecting IEmailService
    // Reason: BackgroundService is Singleton, directly injecting Transient/Scoped services causes lifecycle issues
    public BackgroundEmailService(
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay 10 seconds after startup to avoid resource competition during application startup
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        // Create service scope and send email once
        using var scope = _scopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            await SendNotificationEmail(emailService);
            _logger.LogInformation("Background email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send background email");
        }
        // Task completed, service ends naturally
    }

    private async Task SendNotificationEmail(IEmailService emailService)
    {
        await emailService.SendHtmlEmailAsync(
            "admin@company.com",
            "System Startup Notification",
            @"
            <h2>System Startup Notification</h2>
            <p>Hello,</p>
            <p>The system has started successfully and is now running.</p>
            <p>Startup Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
            <p>Best regards,<br>System Administration</p>
            "
        );
    }
}

// Register the background service
builder.Services.AddHostedService<BackgroundEmailService>();
```

## Configuration Options

### Complete Configuration Example

```json
{
  "EmailConfig": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "UseStartTls": true,
    "UserName": "smtp-username",
    "Password": "smtp-password",
    "From": {
      "Address": "noreply@example.com",
      "Name": "My Application"
    },
    "Bcc": [
      {
        "Address": "audit@example.com",
        "Name": "Audit Trail"
      },
      {
        "Address": "backup@example.com",
        "Name": "Email Backup"
      }
    ]
  }
}
```

## Error Handling and Logging

```csharp
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", 
                request.Email, request.Subject);

            await _emailService.SendHtmlEmailAsync(
                request.Email,
                request.Subject,
                request.Body
            );

            _logger.LogInformation("Email sent successfully to {Email}", request.Email);
            return Ok(new { success = true });
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError(ex, "Email authentication failed");
            return StatusCode(500, new { error = "Email service authentication failed" });
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error occurred while sending email");
            return StatusCode(500, new { error = "Email sending failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Email}", request.Email);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
}

public record EmailRequest(string Email, string Subject, string Body);
```

## Best Practices

1. **Configuration Security**: Store email credentials securely using Azure Key Vault or user secrets
2. **Environment-Specific Settings**: Use different email configurations for development, staging, and production  
3. **Error Handling**: Always implement proper exception handling for email operations
4. **Logging**: Utilize built-in logging to track email sending operations
5. **Background Processing**: For bulk email operations, consider using background services
6. **Resource Management**: The service automatically handles resource disposal

## Dependencies

- **[Linger.Email](../Linger.Email)**: Core email functionality
- **[Linger.Configuration](../Linger.Configuration)**: Configuration extensions
- **Microsoft.Extensions.Logging.Abstractions**: Logging integration
- **Microsoft.Extensions.Options.ConfigurationExtensions**: Configuration binding

## Supported .NET Versions

- .NET 9.0
- .NET 8.0

## Core Documentation

📖 **For core email functionality and detailed API documentation, see: [Linger.Email README](../Linger.Email/README.md)**

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
