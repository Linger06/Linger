# Linger.Email

## 概述

Linger.Email 是用于邮件发送的框架无关核心包，基于 MailKit 构建。它提供 SMTP 传输、邮件内容组装、附件处理和异步发送能力，可在多个 .NET 框架中使用。

## 安装

```bash
dotnet add package Linger.Email
```

## 功能亮点

- **SMTP 发送**：直接通过 MailKit 发送邮件
- **邮件组装**：支持文本和 HTML 邮件，以及多个收件人
- **附件处理**：支持文件路径附件和流附件
- **安全传输**：支持 SSL/TLS 安全发送
- **异步操作**：支持 async/await，避免阻塞应用程序
- **优先级设置**：可配置邮件优先级（高、普通、低）
- **跨平台**：支持多个 .NET 框架（net10.0、net9.0、net8.0、netstandard2.0)

## 快速上手

### 配置邮件服务

```csharp
using Linger.Email;

// 设置邮件服务参数
var emailConfig = new EmailConfig
{
    Host = "smtp.gmail.com",        // SMTP 服务器地址
    Port = 587,                     // 端口号
    UseSsl = true,                  // 启用 SSL
    UseStartTls = true,             // 启用 STARTTLS
    UserName = "your-email@gmail.com",  // 发送账号
    Password = "your-app-password",     // 应用密码
    From = new EmailAddress("your-email@gmail.com", "您的名字")  // 发件人信息
};

// 创建邮件客户端
using var email = new Email(emailConfig);
```

### 发送简单邮件

```csharp
// 准备邮件内容
var message = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com", "收件人姓名") },
    Subject = "来自 Linger.Email 的问候",
    Body = "这是一封测试邮件，希望您一切都好！",
    IsHtmlBody = false  // 纯文本格式
};

// 发送邮件
await email.SendAsync(message);

// 支持取消操作的邮件发送
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await email.SendAsync(message, cancellationToken: cts.Token);
```

### 发送漂亮的 HTML 邮件

```csharp
var htmlMessage = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "欢迎使用我们的服务！",
    Body = @"
        <h1>欢迎！</h1>
        <p>感谢您注册我们的服务，这是您收到的第一封邮件。</p>
        <p>我们为您提供以下功能：</p>
        <ul>
            <li>✅ 安全可靠的服务</li>
            <li>✅ 24/7 技术支持</li>
            <li>✅ 丰富的功能特性</li>
        </ul>
        <p>祝您使用愉快！</p>
    ",
    IsHtmlBody = true  // HTML 格式
};

await email.SendAsync(htmlMessage);
```

### 带附件的邮件

```csharp
var messageWithAttachments = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "重要文件请查收",
    Body = "附件中包含了您需要的文档，请及时查看。",
    IsHtmlBody = false,
    AttachmentsPath = new List<string>
    {
        @"C:\Documents\重要报告.pdf",
        @"C:\Images\图表.png"
    }
};

await email.SendAsync(messageWithAttachments);
```

### 使用内存流作为附件

```csharp
// 当您需要发送动态生成的文件时，这种方式非常有用
var attachmentInfos = new List<AttachmentInfo>
{
    new()
    {
        Stream = new MemoryStream(pdfBytes),    // PDF 文件的字节数组
        FileName = "动态生成的报告.pdf",
        MediaType = "application/pdf"
    },
    new()
    {
        Stream = imageStream,                   // 图片流
        FileName = "统计图表.jpg",
        MediaType = "image/jpeg"
    }
};

var message = new EmailMessage
{
    To = new List<EmailAddress> { new("recipient@example.com") },
    Subject = "系统生成的报告",
    Body = "这些文件是系统自动生成的，请查收。",
    Attachments = attachmentInfos
};

await email.SendAsync(message);
```

## ASP.NET Core 集成

如果你在 ASP.NET Core 项目中使用，请查看 [Linger.Email.AspNetCore](../Linger.Email.AspNetCore/README.zh-CN.md)，它提供依赖注入、配置绑定和日志集成。

## 进阶用法

### 群发邮件（抄送和密送）

```csharp
var message = new EmailMessage
{
    To = new List<EmailAddress>  // 主要收件人
    {
        new("张三@example.com", "张三"),
        new("李四@example.com", "李四")
    },
    Cc = new List<EmailAddress>  // 抄送给
    {
        new("manager@example.com", "部门经理")
    },
    Bcc = new List<EmailAddress>  // 密送给（其他人看不到）
    {
        new("archive@example.com", "邮件存档")
    },
    Subject = "重要：团队会议通知",
    Body = "各位同事，下周一上午10点召开项目会议...",
    Priority = MessagePriority.High  // 设为高优先级
};

await email.SendAsync(message);
```

### 发送完成后的回调处理

```csharp
await email.SendAsync(message, response =>
{
    Console.WriteLine($"邮件发送完成！服务器响应：{response}");
    // 可以在这里记录日志或执行其他操作
});
```

### 常用邮箱的配置方法

#### Gmail 配置
```csharp
var gmailConfig = new EmailConfig
{
    Host = "smtp.gmail.com",
    Port = 587,
    UseSsl = true,
    UseStartTls = true,
    UserName = "your-email@gmail.com",
    Password = "your-app-password",  // 注意：这里要用应用专用密码，不是登录密码
    From = new EmailAddress("your-email@gmail.com", "您的名字")
};
```

#### Outlook/Hotmail 配置
```csharp
var outlookConfig = new EmailConfig
{
    Host = "smtp-mail.outlook.com",
    Port = 587,
    UseSsl = false,
    UseStartTls = true,
    UserName = "your-email@outlook.com",
    Password = "your-password",
    From = new EmailAddress("your-email@outlook.com", "您的名字")
};
```

#### 企业邮箱配置
```csharp
var customConfig = new EmailConfig
{
    Host = "mail.your-company.com",  // 您公司的邮件服务器
    Port = 25,                       // 根据实际情况调整
    UseSsl = false,
    UseStartTls = false,
    UserName = "username",
    Password = "password",
    From = new EmailAddress("noreply@your-company.com", "系统邮件")
};
```

### 全局密送设置

```csharp
// 如果需要将所有邮件都密送一份给特定邮箱（比如存档或监管需要）
var config = new EmailConfig
{
    Host = "smtp.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "sender@example.com",
    Password = "password",
    From = new EmailAddress("sender@example.com", "系统发件人"),
    Bcc = new List<EmailAddress>  // 全局密送列表
    {
        new("audit@example.com", "邮件审计"),
        new("backup@example.com", "邮件备份")
    }
};
```

## 错误处理小贴士

```csharp
try
{
    await email.SendAsync(message);
    Console.WriteLine("邮件发送成功！");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"登录失败，请检查用户名和密码：{ex.Message}");
    // 通常是用户名或密码错误
}
catch (SmtpException ex)
{
    Console.WriteLine($"邮件服务器出现问题：{ex.Message}");
    // 可能是网络问题或服务器设置不正确
}
catch (Exception ex)
{
    Console.WriteLine($"发送失败：{ex.Message}");
    // 其他未预期的错误
}
```

## 使用建议

### 安全性方面
- **使用应用密码**：Gmail 等邮箱需要生成应用专用密码，不要用登录密码
- **妥善保管凭据**：将邮箱密码存储在配置文件或密钥管理服务中，别写死在代码里
- **验证邮箱格式**：发送前简单检查一下邮箱地址格式，避免无效发送

### 取消操作支持

所有异步邮件操作都支持 `CancellationToken`，让您能够优雅地取消操作：

```csharp
public class EmailService
{
    private readonly IEmail _email;
    
    public EmailService(IEmail email)
    {
        _email = email;
    }
    
    // 带超时的邮件发送
    public async Task<bool> SendEmailWithTimeoutAsync(EmailMessage message, int timeoutSeconds = 30)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        try
        {
            await _email.SendAsync(message, cancellationToken: cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("邮件发送超时被取消");
            return false;
        }
    }
    
    // 支持外部取消的批量发送
    public async Task SendBulkEmailsAsync(
        List<EmailMessage> messages, 
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            // 检查是否请求取消
            cancellationToken.ThrowIfCancellationRequested();
            
            await _email.SendAsync(message, cancellationToken: cancellationToken);
            
            // 可选：邮件之间添加延迟
            await Task.Delay(1000, cancellationToken);
        }
    }
}
```

### 在 ASP.NET Core 中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail(
        [FromBody] EmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 传递请求的取消令牌
            // 如果客户端断开连接会自动取消
            await _emailService.SendTextEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                cancellationToken
            );
            
            return Ok("邮件发送成功");
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, "请求被客户端取消");
        }
    }
}
```

### 性能优化
- **复用连接**：发送多封邮件时，使用同一个 Email 实例，减少连接开销
- **批量发送**：有大量邮件时，分批发送效果更好
- **后台处理**：重要业务流程不要被邮件发送阻塞，可以放到后台队列处理
- **使用取消令牌**：利用取消令牌避免在已取消操作上浪费资源

### 资源管理
- **及时释放**：使用 `using` 语句确保资源正确释放
- **处理大附件**：发送大文件时注意内存使用，考虑分段或压缩
- **重试机制**：网络不稳定时，增加简单的重试逻辑

## 实际应用场景

### 用户注册欢迎邮件
```csharp
var welcomeMessage = new EmailMessage
{
    To = new List<EmailAddress> { new(userEmail, userName) },
    Subject = "欢迎加入我们！🎉",
    Body = $@"
        <h2>欢迎您，{userName}！</h2>
        <p>感谢您的注册，我们很高兴为您服务。</p>
        <p>您现在可以开始探索我们的功能了：</p>
        <ul>
            <li>浏览产品目录</li>
            <li>查看个人资料</li>
            <li>联系客服支持</li>
        </ul>
        <p>有任何问题随时联系我们！</p>
    ",
    IsHtmlBody = true
};

await email.SendAsync(welcomeMessage);
```

### 密码重置邮件
```csharp
var resetMessage = new EmailMessage
{
    To = new List<EmailAddress> { new(userEmail, userName) },
    Subject = "重置您的账户密码",
    Body = $@"
        <h2>密码重置请求</h2>
        <p>您好 {userName}，</p>
        <p>我们收到了您的密码重置请求。</p>
        <p><a href='{resetLink}' style='color: #007bff;'>点击这里重置密码</a></p>
        <p><small>⏰ 此链接 24 小时内有效</small></p>
        <p>如果不是您本人操作，请忽略此邮件。</p>
    ",
    IsHtmlBody = true
};

await email.SendAsync(resetMessage);
```

### 订单确认邮件
```csharp
var orderMessage = new EmailMessage
{
    To = new List<EmailAddress> { new(customerEmail, customerName) },
    Subject = $"订单确认 #{orderNumber} - 感谢您的购买",
    Body = $@"
        <h2>订单确认</h2>
        <p>亲爱的 {customerName}，</p>
        <p>您的订单已确认，我们正在准备发货。</p>
        <p><strong>订单号：</strong>{orderNumber}</p>
        <p><strong>总金额：</strong>¥{totalAmount}</p>
        <p>详细信息请查看附件中的发票。</p>
    ",
    IsHtmlBody = true,
    AttachmentsPath = new List<string> { invoicePdfPath }
};

await email.SendAsync(orderMessage);
```

### 系统监控警告
```csharp
var alertMessage = new EmailMessage
{
    To = adminEmails,
    Subject = "⚠️ 系统性能警告",
    Body = $@"
        <h2>系统警告</h2>
        <p>检测到系统性能异常：</p>
        <ul>
            <li>CPU 使用率：{cpuUsage}%</li>
            <li>内存使用率：{memoryUsage}%</li>
            <li>发生时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
        </ul>
        <p>请及时检查服务器状态。</p>
    ",
    IsHtmlBody = true,
    Priority = MessagePriority.High
};

await email.SendAsync(alertMessage);
```
## 主要组件说明

### EmailConfig 配置类
邮件服务的基础配置，包含服务器地址、端口、安全设置等信息。设置一次就能重复使用。

### EmailMessage 邮件对象
代表一封完整的邮件，包含收件人、主题、正文、附件等所有信息。支持纯文本和 HTML 格式。

### EmailAddress 邮箱地址
封装邮箱地址和显示名称，让收件人看到更友好的发件人信息。

### AttachmentInfo 附件信息
支持文件路径附件和内存流附件两种方式，适应不同的使用场景。

### Email 邮件客户端
核心的邮件发送类，负责与 SMTP 服务器建立连接并发送邮件。

## 技术依赖

- **[MailKit](https://github.com/jstedfast/MailKit)**：业界领先的 .NET 邮件处理库，支持 IMAP、POP3 和 SMTP
- **[MimeKit](https://github.com/jstedfast/MimeKit)**：强大的 MIME 消息处理库，确保邮件格式标准

## 框架支持

 - .NET 10.0
 - .NET 9.0
 - .NET 8.0
 - .NET Standard 2.0

兼容 Windows、Linux、macOS 等多个平台。

## 相关项目

📖 **想要在 ASP.NET Core 中使用？** 请查看：[Linger.Email.AspNetCore README](../Linger.Email.AspNetCore/README.md)，提供了依赖注入和配置管理功能。
