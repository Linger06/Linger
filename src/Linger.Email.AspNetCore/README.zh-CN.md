# Linger.Email.AspNetCore

> 📝 *查看此文档的其他语言版本：[English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.Email.AspNetCore 是专为 ASP.NET Core 应用设计的邮件服务扩展包。它让您在 Web 应用中发送邮件变得轻松简单——通过依赖注入自动管理邮件客户端，从配置文件直接读取设置，还提供了许多开箱即用的便捷功能。

## 安装

```bash
dotnet add package Linger.Email.AspNetCore
```

## 功能亮点

- **一键集成**：几行代码就能在项目中启用邮件功能
- **配置简单**：直接从 appsettings.json 读取配置，无需手动设置
- **依赖注入友好**：完美融入 ASP.NET Core 的服务容器
- **便捷方法**：提供专门的快捷方法处理常见邮件场景
- **自动日志记录**：邮件发送过程自动记录，便于调试和监控
- **配置验证**：启动时自动检查配置，及早发现问题
- **现代化支持**：兼容 .NET 9.0 和 .NET 8.0

## 快速上手

### 1. 配置邮件参数

在项目的 `appsettings.json` 文件中添加邮件配置：

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
      "Name": "你的应用程序"
    },
    "Bcc": [
      {
        "Address": "audit@yourcompany.com",
        "Name": "审计跟踪"
      }
    ]
  }
}
```

### 2. 启用邮件服务

在 `Program.cs` 文件中注册邮件服务（.NET 6+ 风格）：

```csharp
using Linger.Email.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 推荐方式：自动读取配置并注册服务
builder.Services.ConfigureEmail(builder.Configuration);

// 备选方式：使用 MailKit 的配置方式
// builder.Services.ConfigureMailKit(builder.Configuration);

var app = builder.Build();
```

### 3. 在控制器中发送邮件

```csharp
using Linger.Email.AspNetCore;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IEmailService _emailService;

    public NotificationController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("welcome")]
    public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeEmailRequest request)
    {
        try
        {
            await _emailService.SendHtmlEmailAsync(
                request.Email,
                "🎉 欢迎加入我们！",
                $@"
                <h1>您好，{request.Name}！</h1>
                <p>非常欢迎您注册我们的服务，我们很高兴为您提供帮助。</p>
                <p>现在您就可以开始探索我们的各种功能了！</p>
                <p>祝您使用愉快！<br>产品团队</p>
                "
            );

            return Ok(new { message = "欢迎邮件已成功发送" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"邮件发送失败：{ex.Message}" });
        }
    }

    [HttpPost("notification")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        await _emailService.SendTextEmailAsync(
            request.Email,
            request.Subject,
            request.Message
        );

        return Ok(new { message = "通知邮件已发送" });
    }
    }
}

public record WelcomeEmailRequest(string Email, string Name);
public record NotificationRequest(string Email, string Subject, string Message);
```

## 进阶用法

### IEmailService 提供的便捷方法

`IEmailService` 针对常见的邮件发送场景提供了简化的方法：

```csharp
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // 发送简单文本邮件
    public async Task SendSimpleText()
    {
        await _emailService.SendTextEmailAsync(
            "user@example.com",
            "重要通知",
            "您的订单已确认，我们正在处理中。感谢您的耐心等待！"
        );
    }

    // 发送精美的 HTML 邮件
    public async Task SendRichHtml()
    {
        await _emailService.SendHtmlEmailAsync(
            "user@example.com",
            "账户激活成功",
            @"
            <div style='font-family: Arial, sans-serif;'>
                <h1 style='color: #4CAF50;'>🎉 激活成功！</h1>
                <p>您的账户已成功激活，现在可以享受完整的服务功能。</p>
                <p>如有任何问题，请随时联系我们的客服团队。</p>
            </div>"
        );
    }

    // 发送带文件的邮件
    public async Task SendWithFiles()
    {
        var attachmentPaths = new[]
        {
            @"C:\Reports\monthly_report.pdf",
            @"C:\Images\chart.png"
        };

        await _emailService.SendWithAttachmentsAsync(
            "manager@example.com",
            "月度报告已生成",
            "<p>您好！本月的业务报告已生成完毕，请查看附件。</p>",
            isHtml: true,
            attachmentPaths
        );
    }
}
```

### 复杂邮件场景

当需要更精细的控制时，可以使用完整的 `SendAsync` 方法：

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
                new("primary@example.com", "主要收件人"),
                new("secondary@example.com", "次要收件人")
            },
            Cc = new List<EmailAddress>
            {
                new("manager@example.com", "经理")
            },
            Subject = "项目状态更新",
            Body = @"
                <h2>项目状态报告</h2>
                <p>团队您好，</p>
                <p>以下是我们项目的最新状态：</p>
                <ul>
                    <li>✅ 第一阶段：已完成</li>
                    <li>🔄 第二阶段：进行中</li>
                    <li>⏳ 第三阶段：待开始</li>
                </ul>
                <p>此致，<br>项目经理</p>
            ",
            IsHtmlBody = true,
            Priority = MessagePriority.High
        };

        await _emailService.SendAsync(message, response =>
        {
            // 处理发送完成的可选回调
            Console.WriteLine($"邮件已发送: {response}");
        });
    }
}
```

### 后台邮件服务

对于在后台任务中发送邮件：

```csharp
public class BackgroundEmailService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundEmailService> _logger;

    // 重要：使用 IServiceScopeFactory 而不是直接注入 IEmailService
    // 原因：BackgroundService 是 Singleton，直接注入 Transient/Scoped 服务会导致生命周期问题
    public BackgroundEmailService(
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 启动后延迟10秒再发送邮件，避免应用启动时的资源竞争
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        // 创建服务作用域并发送一次邮件
        using var scope = _scopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            await SendNotificationEmail(emailService);
            _logger.LogInformation("后台邮件发送完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "后台邮件发送失败");
        }
        // 任务完成，服务自然结束
    }

    private async Task SendNotificationEmail(IEmailService emailService)
    {
        await emailService.SendHtmlEmailAsync(
            "admin@company.com",
            "系统启动通知",
            @"
            <h2>系统启动通知</h2>
            <p>您好，</p>
            <p>系统已成功启动并运行。</p>
            <p>启动时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
            <p>此致，<br>系统管理</p>
            "
        );
    }
}

// 注册后台服务
builder.Services.AddHostedService<BackgroundEmailService>();
```

## 配置选项

### 完整配置示例

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
      "Name": "我的应用程序"
    },
    "Bcc": [
      {
        "Address": "audit@example.com",
        "Name": "审计跟踪"
      },
      {
        "Address": "backup@example.com",
        "Name": "邮件备份"
      }
    ]
  }
}
```

## 错误处理和日志记录

### 内置日志记录

`EmailService` 类已内置日志记录功能：

```csharp
// EmailService 会自动记录邮件发送状态
// 成功日志：正在发送邮件到 {Recipients}
// 完成日志：邮件发送成功: {Response}  
// 错误日志：发送邮件失败: {Error}
```

### 在控制器中处理异常

```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        IEmailService emailService, 
        ILogger<NotificationController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send-notification")]
    public async Task<IActionResult> SendNotification([FromBody] EmailRequest request)
    {
        try
        {
            await _emailService.SendHtmlEmailAsync(
                request.Email,
                request.Subject,
                request.Body
            );

            return Ok(new { message = "邮件发送成功" });
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP 错误发送邮件到 {Email}", request.Email);
            return StatusCode(500, new { error = "邮件发送失败" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送邮件时发生意外错误 {Email}", request.Email);
            return StatusCode(500, new { error = "发生意外错误" });
        }
    }
}

public record EmailRequest(string Email, string Subject, string Body);
```

## 最佳实践

1. **配置管理**：使用 Azure Key Vault 或用户机密存储 SMTP 凭据
2. **错误处理**：正确处理 SMTP 异常和网络错误
3. **日志记录**：利用内置的日志记录功能进行问题诊断
4. **资源管理**：服务会自动处理资源释放
5. **性能优化**：对于大量邮件，考虑使用后台服务

## 依赖项

- **[Linger.Email](../Linger.Email)**：核心邮件功能
- **[Linger.Configuration](../Linger.Configuration)**：配置扩展
- **Microsoft.Extensions.Logging.Abstractions**：日志集成
- **Microsoft.Extensions.Options.ConfigurationExtensions**：配置绑定

## 支持的 .NET 版本

- .NET 9.0
- .NET 8.0

## 核心文档

📖 **有关核心邮件功能和详细 API 文档，请参阅：[Linger.Email README](../Linger.Email/README.zh-CN.md)**

## 许可证

本项目按照 Linger 项目提供的许可证条款进行许可。

## 贡献

欢迎贡献！请随时提交 Pull Request。