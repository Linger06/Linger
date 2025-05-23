# Linger.Email.AspNetCore

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.Email.AspNetCore 提供了 ASP.NET Core 与 Linger.Email 库的集成，使在 ASP.NET Core 应用程序中发送电子邮件变得简单。它包括依赖注入扩展和配置集成，简化了电子邮件的设置和管理。

## 支持的 .NET 版本

- .NET 9.0
- .NET 8.0

## 安装

```bash
dotnet add package Linger.Email.AspNetCore
```

## 功能特点

- 与 ASP.NET Core 依赖注入的简单集成
- 从 appsettings.json 进行配置绑定
- 支持多个命名电子邮件配置
- 日志集成
- 完全兼容 Linger.Email 功能

## 基本用法

### 配置

将电子邮件设置添加到您的 `appsettings.json`：

```json
{
  "EmailOptions": {
    "DefaultFromEmail": "noreply@example.com",
    "DefaultFromName": "我的应用程序",
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "Username": "username",
      "Password": "password",
      "EnableSsl": true
    }
  }
}
```

### 服务注册

在 `Program.cs` 或 `Startup.cs` 中：

```csharp
using Linger.Email.AspNetCore;
using Linger.Email;

var builder = WebApplication.CreateBuilder(args);

// 添加邮件服务
builder.Services.AddEmailServices(builder.Configuration);

// 或使用自定义配置
builder.Services.AddEmailServices(options => {
    options.DefaultFromEmail = "noreply@example.com";
    options.DefaultFromName = "我的应用程序";
    options.Smtp = new SmtpOptions {
        Host = "smtp.example.com",
        Port = 587,
        Username = "username",
        Password = "password",
        EnableSsl = true
    };
});
```

### 使用电子邮件服务

```csharp
using Linger.Email;

public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    
    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailModel model)
    {
        var message = new EmailMessage
        {
            To = model.To,
            Subject = model.Subject,
            Body = model.Body,
            IsBodyHtml = true
        };
        
        await _emailSender.SendAsync(message);
        return Ok("电子邮件发送成功");
    }
}
```

## 高级功能

### 多个电子邮件配置

配置多个电子邮件提供程序：

```csharp
// 在启动时
builder.Services.AddEmailServices(options => {
    // 配置默认选项
})
.AddNamedEmailOptions("marketing", options => {
    options.DefaultFromEmail = "marketing@example.com";
    options.DefaultFromName = "营销团队";
    // 其他设置...
})
.AddNamedEmailOptions("support", options => {
    options.DefaultFromEmail = "support@example.com";
    options.DefaultFromName = "支持团队";
    // 其他设置...
});
```

使用命名电子邮件发送器：

```csharp
public class NotificationService
{
    private readonly IEmailSenderFactory _emailSenderFactory;
    
    public NotificationService(IEmailSenderFactory emailSenderFactory)
    {
        _emailSenderFactory = emailSenderFactory;
    }
    
    public async Task SendMarketingEmail(string to, string subject, string body)
    {
        var emailSender = _emailSenderFactory.Create("marketing");
        
        var message = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        
        await emailSender.SendAsync(message);
    }
}
```

## 依赖项

- [Linger.Email](../Linger.Email)：核心电子邮件功能
- [Linger.Configuration](../Linger.Configuration)：配置抽象
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
