# Linger.Email

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

## Usage Examples

### Basic Email Sending
```csharp
// Simple text email 
var email = new EmailMessage 
{ 
    From = "sender@example.com", 
    To = new[] { "recipient@example.com" }, 
    Subject = "Hello", 
    Body = "This is a test email" 
};
await emailService.SendAsync(email);

// Multiple recipients 
var groupEmail = new EmailMessage 
{ 
    From = "sender@example.com", 
    To = new[] { "recipient1@example.com", "recipient2@example.com" }, 
    Cc = new[] { "manager@example.com" }, 
    Bcc = new[] { "archive@example.com" }, 
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
    From = "reports@company.com", 
    To = new[] { "manager@company.com" },
    Subject = "Monthly Report", 
    IsHtml = true,
    Body = @" Monthly Sales Report Please find the attached report for this month.  Total Sales: $50,000 Growth: 15% " 
};
reportEmail.Attachments.Add(new EmailAttachment("monthly-report.pdf"));
await emailService.SendAsync(reportEmail);

// Multiple attachments with custom names 
var documentsEmail = new EmailMessage
{ 
    From = "documents@company.com", 
    To = new[] { "client@example.com" }, 
    Subject = "Project Documentation", 
    IsHtml = true, 
    Body = "Please find attached the project documents.", 
    Attachments = new List { new EmailAttachment("specs.pdf", "Project-Specifications.pdf"), new EmailAttachment("timeline.xlsx", "Project-Timeline.xlsx") }
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