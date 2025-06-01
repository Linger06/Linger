# Linger.Email

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.Email is a comprehensive C# email helper library that provides simplified email operations with modern .NET development in mind. Built on top of the robust MailKit library, it offers secure SMTP support, HTML and plain text emails, attachment handling, and asynchronous sending capabilities across multiple .NET frameworks.

## Installation

```bash
dotnet add package Linger.Email
```

## Features

- **Multi-format Email Support**: Send both HTML and plain text emails
- **Attachment Handling**: Support for file attachments with multiple formats
- **Secure SMTP**: SSL/TLS encryption support for secure email transmission
- **Asynchronous Operations**: Modern async/await pattern for non-blocking email operations
- **Flexible Configuration**: Easy-to-use configuration system
- **Multiple Recipients**: Support for To, CC, and BCC recipients
- **Priority Levels**: Configure email priority (High, Normal, Low)
- **Cross-platform**: Supports multiple .NET frameworks (net9.0, net8.0, netstandard2.0)

## Quick Start

### Basic Configuration

```csharp
using Linger.Email;

// Configure email settings
var emailConfig = new EmailConfig
{
    Host = "smtp.gmail.com",
    Port = 587,
    UseSsl = true,
    UseStartTls = true,
    UserName = "your-email@gmail.com",
    Password = "your-app-password",
    From = new EmailAddress("your-email@gmail.com", "Your Name")
};

// Create email client
using var email = new Email(emailConfig);
```

### Send Simple Text Email

```csharp
// Create email message
var message = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com", "Recipient Name") },
    Subject = "Hello from Linger.Email",
    Body = "This is a simple text email.",
    IsHtmlBody = false
};

// Send email
await email.SendAsync(message);
```

### Send HTML Email

```csharp
var htmlMessage = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "HTML Email Example",
    Body = @"
        <h1>Welcome!</h1>
        <p>This is an <strong>HTML email</strong> sent using Linger.Email.</p>
        <ul>
            <li>Feature 1</li>
            <li>Feature 2</li>
            <li>Feature 3</li>
        </ul>
    ",
    IsHtmlBody = true
};

await email.SendAsync(htmlMessage);
```

### Send Email with Attachments

```csharp
var messageWithAttachments = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "Email with Attachments",
    Body = "Please find the attached files.",
    IsHtmlBody = false,
    AttachmentsPath = new List<string>
    {
        @"C:\Documents\report.pdf",
        @"C:\Images\chart.png"
    }
};

await email.SendAsync(messageWithAttachments);
```

### Send Email with Stream Attachments

```csharp
// Using AttachmentInfo for stream-based attachments
var attachmentInfos = new List<AttachmentInfo>
{
    new()
    {
        Stream = new MemoryStream(pdfBytes),
        FileName = "generated-report.pdf",
        MediaType = "application/pdf"
    },
    new()
    {
        Stream = imageStream,
        FileName = "image.jpg",
        MediaType = "image/jpeg"
    }
};

var message = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "Email with Stream Attachments",
    Body = "Generated attachments from streams.",
    Attachments = attachmentInfos
};

await email.SendAsync(message);
```

## Advanced Features

### Multiple Recipients with CC and BCC

```csharp
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
    Bcc = new List<EmailAddress>
    {
        new("archive@example.com", "Archive")
    },
    Subject = "Team Update",
    Body = "Important team announcement...",
    Priority = MessagePriority.High
};

await email.SendAsync(message);
```

### Email with Callback

```csharp
await email.SendAsync(message, response =>
{
    Console.WriteLine($"Email sent successfully: {response}");
    // Log the response or perform additional actions
});
```

### Different SMTP Configurations

#### Gmail Configuration
```csharp
var gmailConfig = new EmailConfig
{
    Host = "smtp.gmail.com",
    Port = 587,
    UseSsl = true,
    UseStartTls = true,
    UserName = "your-email@gmail.com",
    Password = "your-app-password", // Use App Password, not regular password
    From = new EmailAddress("your-email@gmail.com", "Your Name")
};
```

#### Outlook Configuration
```csharp
var outlookConfig = new EmailConfig
{
    Host = "smtp-mail.outlook.com",
    Port = 587,
    UseSsl = false,
    UseStartTls = true,
    UserName = "your-email@outlook.com",
    Password = "your-password",
    From = new EmailAddress("your-email@outlook.com", "Your Name")
};
```

#### Custom SMTP Server
```csharp
var customConfig = new EmailConfig
{
    Host = "mail.your-domain.com",
    Port = 25,
    UseSsl = false,
    UseStartTls = false,
    UserName = "username",
    Password = "password",
    From = new EmailAddress("noreply@your-domain.com", "Your App Name")
};
```

### Global BCC Configuration

```csharp
var config = new EmailConfig
{
    Host = "smtp.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "sender@example.com",
    Password = "password",
    From = new EmailAddress("sender@example.com", "Sender"),
    Bcc = new List<EmailAddress>
    {
        new("audit@example.com", "Audit Trail"),
        new("backup@example.com", "Backup Archive")
    }
};
```

## Error Handling

```csharp
try
{
    await email.SendAsync(message);
    Console.WriteLine("Email sent successfully!");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
    // Handle authentication errors
}
catch (SmtpException ex)
{
    Console.WriteLine($"SMTP error: {ex.Message}");
    // Handle SMTP-specific errors
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
    // Handle other errors
}
```

## Best Practices

1. **Use App Passwords**: For Gmail and other providers, use app-specific passwords instead of regular passwords
2. **Dispose Properly**: Always use `using` statements or proper disposal patterns
3. **Validate Email Addresses**: Validate email addresses before sending
4. **Handle Exceptions**: Implement proper exception handling for network and authentication issues
5. **Stream Management**: Properly dispose of streams when using stream-based attachments
6. **Configuration Security**: Store email credentials securely (e.g., Azure Key Vault, user secrets)

## Performance Tips

1. **Connection Reuse**: Reuse SMTP connections for better performance when sending multiple emails
2. **Batch Processing**: Send emails in batches to reduce connection overhead
3. **Async Processing**: Use background services for high-volume email processing
4. **Configuration Optimization**: Set appropriate timeout and retry policies
5. **Resource Management**: Properly dispose of resources, especially when using streams
6. **Error Handling**: Implement retry logic for transient failures

## Common Use Cases

### Password Reset Email
```csharp
var resetMessage = new EmailMessage
{
    To = new List<EmailAddress> { new(userEmail, userName) },
    Subject = "Password Reset Request",
    Body = $@"
        <h2>Password Reset</h2>
        <p>Hello {userName},</p>
        <p>Click the link below to reset your password:</p>
        <a href='{resetLink}'>Reset Password</a>
        <p>This link expires in 24 hours.</p>
    ",
    IsHtmlBody = true
};

await email.SendAsync(resetMessage);
```

### Order Confirmation Email
```csharp
var orderMessage = new EmailMessage
{
    To = new List<EmailAddress> { new(customerEmail, customerName) },
    Subject = $"Order Confirmation - #{orderNumber}",
    Body = GenerateOrderConfirmationHtml(order),
    IsHtmlBody = true,
    AttachmentsPath = new List<string> { invoicePdfPath }
};

await email.SendAsync(orderMessage);
```

### System Notification Email
```csharp
var notificationMessage = new EmailMessage
{
    To = adminEmails,
    Subject = "System Alert: High CPU Usage",
    Body = "System performance alert...",
    Priority = MessagePriority.High
};

await email.SendAsync(notificationMessage);
```

## Core Classes

### EmailConfig
Configuration class that holds SMTP server settings, authentication credentials, and default sender information.

### EmailMessage
Represents an email message with recipients, subject, body, attachments, and other properties.

### EmailAddress
Represents an email address with optional display name.

### AttachmentInfo
Represents an email attachment with stream, filename, and media type information.

### Email
Main email client class that handles SMTP connections and message sending.

## Dependencies

- **[MailKit](https://github.com/jstedfast/MailKit)**: A cross-platform .NET library for IMAP, POP3, and SMTP
- **[MimeKit](https://github.com/jstedfast/MimeKit)**: A .NET MIME creation and parser library

## Supported .NET Versions

- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## Integration

📖 **For ASP.NET Core integration and dependency injection support, see: [Linger.Email.AspNetCore README](../Linger.Email.AspNetCore/README.md)**

## License

This project is licensed under the terms of the license provided with the Linger project.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

