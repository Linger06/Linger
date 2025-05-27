# Linger.Email

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A C# email helper library that provides simplified email operations and SMTP support for .NET Framework 4.6.2+ and .NET Standard 2.0+.

## Introduction

Linger.Email simplifies email operations in .NET applications by providing an easy-to-use interface for sending emails, managing attachments, and handling SMTP configurations.

## Features

### Email Operations
- Simple email sending with fluent API
- HTML and plain text support
- File attachment handling
- Email template support
- Async operations

### SMTP Configuration
- Multiple SMTP server support
- SSL/TLS encryption
- Custom port configuration
- Authentication options

## Setup & Configuration

Before using Linger.Email, you need to configure the email client. There are two main approaches:

### Direct instantiation (for non-ASP.NET Core projects)

```csharp
// Create email configuration
var emailConfig = new EmailConfig
{
    Host = "smtp.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "username",
    Password = "password",
    From = new EmailAddress { Address = "noreply@example.com", Name = "Example System" }
};

// Create email service
var emailService = new Email(emailConfig);

// Now you can use emailService to send emails
```

### Dependency Injection (for ASP.NET Core projects)

For ASP.NET Core projects, please use the [Linger.Email.AspNetCore](../Linger.Email.AspNetCore) package to simplify email service configuration and dependency injection. Refer to that package's documentation for detailed usage.
```

## Usage Examples

### Basic Email Sending
```csharp
// Simple text email 
var email = new EmailMessage 
{ 
    From = new EmailAddress { Address = "sender@example.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "recipient@example.com" } }, 
    Subject = "Hello", 
    Body = "This is a test email" 
};
await emailService.SendAsync(email);

// Multiple recipients 
var groupEmail = new EmailMessage 
{ 
    From = new EmailAddress { Address = "sender@example.com" }, 
    To = new List<EmailAddress> 
    { 
        new EmailAddress { Address = "recipient1@example.com" }, 
        new EmailAddress { Address = "recipient2@example.com" } 
    }, 
    Cc = new List<EmailAddress> { new EmailAddress { Address = "manager@example.com" } }, 
    Bcc = new List<EmailAddress> { new EmailAddress { Address = "archive@example.com" } }, 
    Subject = "Team Meeting", 
    Body = "Let's meet tomorrow at 2 PM" 
};
await emailService.SendAsync(groupEmail);
```


### HTML Email with Attachments
```csharp
// HTML email with single attachment 
var reportEmail = new EmailMessage 
{ 
    From = new EmailAddress { Address = "reports@company.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "manager@company.com" } },
    Subject = "Monthly Report", 
    IsHtmlBody = true,
    Body = @"<h1>Monthly Sales Report</h1><p>Please find the attached report for this month.</p><p><strong>Total Sales:</strong> $50,000<br><strong>Growth:</strong> 15%</p>" 
};
// Add file path as attachment
reportEmail.AttachmentsPath = new List<string> { "monthly-report.pdf" };
await emailService.SendAsync(reportEmail);

// Multiple attachments with custom names 
var documentsEmail = new EmailMessage
{ 
    From = new EmailAddress { Address = "documents@company.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "client@example.com" } }, 
    Subject = "Project Documentation", 
    IsHtmlBody = true, 
    Body = "Please find attached the project documents."
};
// Add attachment information
documentsEmail.Attachments = new List<AttachmentInfo> { 
    new AttachmentInfo { FileName = "Project-Specifications.pdf", Stream = File.OpenRead("specs.pdf") },
    new AttachmentInfo { FileName = "Project-Timeline.xlsx", Stream = File.OpenRead("timeline.xlsx") }
};
await emailService.SendAsync(documentsEmail);
```

## Install

### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.Email".
5. Click on the `Linger.Email` package, select the appropriate version and click Install.

### Package Manager Console

```
PM> Install-Package Linger.Email
```

### .NET CLI Console

```
> dotnet add package Linger.Email
```