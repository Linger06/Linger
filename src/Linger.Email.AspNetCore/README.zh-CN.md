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

### 配置与服务注册

1. 将电子邮件设置添加到您的 `appsettings.json`：

```json
{
  "EmailConfig": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "UserName": "username",
    "Password": "password",
    "From": {
      "Name": "我的应用程序",
      "Address": "noreply@example.com"
    }
  }
}
```

2. 在 `Program.cs` 或 `Startup.cs` 中注册服务：

```csharp
using Linger.Email.AspNetCore;
using Linger.Email;

var builder = WebApplication.CreateBuilder(args);

// 方式1：使用 ConfigureEmail 配置服务
builder.Services.ConfigureEmail(builder.Configuration);

// 方式2：或使用 ConfigureMailKit 配置服务
builder.Services.ConfigureMailKit(builder.Configuration);
```

### 使用电子邮件服务

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
        return Ok("电子邮件发送成功");
    }
}
```

## 高级功能

### 多个电子邮件配置

您可以为不同的应用部分配置不同的电子邮件配置：

```csharp
// 在启动时
// 主要电子邮件服务
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("PrimaryEmailConfig"));
services.AddTransient<IEmailService, EmailService>();

// 营销电子邮件服务
var marketingConfig = new EmailConfig
{
    Host = "smtp-marketing.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "marketing",
    Password = "password123",
    From = new EmailAddress { Name = "营销团队", Address = "marketing@example.com" }
};
builder.Services.AddSingleton<MarketingEmailService>(sp => new MarketingEmailService(marketingConfig));
```

使用命名电子邮件发送器：

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

## 依赖项

- [Linger.Email](../Linger.Email)：核心电子邮件功能
- [Linger.Configuration](../Linger.Configuration)：配置抽象
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

## 贡献

欢迎贡献！请随时提交 Pull Request。
