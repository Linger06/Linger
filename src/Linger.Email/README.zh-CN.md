# Linger.Email

## 概述

Linger.Email 是一个专为 .NET 开发者设计的邮件发送工具库。它基于 MailKit 构建，让您能够轻松发送各种类型的邮件——无论是简单的文本通知，还是带有附件的复杂邮件。支持现代异步编程模式，让邮件发送不再阻塞您的应用程序。

## 安装

```bash
dotnet add package Linger.Email
```

## 功能亮点

- **邮件格式多样化**：轻松发送纯文本或精美的 HTML 邮件
- **附件无忧**：支持各种类型的文件附件，操作简单
- **连接安全可靠**：内置 SSL/TLS 加密，保障邮件传输安全
- **异步高效**：采用 async/await 模式，不会阻塞您的程序
- **配置简单**：几行代码就能完成邮件服务配置
- **群发便捷**：支持多个收件人、抄送、密送，一次搞定
- **优先级控制**：可设置邮件的重要程度
- **跨平台兼容**：支持 .NET 9.0、.NET 8.0、.NET Standard 2.0

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

### 性能优化
- **复用连接**：发送多封邮件时，使用同一个 Email 实例，减少连接开销
- **批量发送**：有大量邮件时，分批发送效果更好
- **后台处理**：重要业务流程不要被邮件发送阻塞，可以放到后台队列处理

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

- .NET 9.0 ✅
- .NET 8.0 ✅  
- .NET Standard 2.0 ✅

兼容 Windows、Linux、macOS 等多个平台。

## 相关项目

📖 **想要在 ASP.NET Core 中使用？** 请查看：[Linger.Email.AspNetCore README](../Linger.Email.AspNetCore/README.md)，提供了依赖注入和配置管理功能。

## 开源许可

本项目遵循 Linger 项目的开源许可协议。

## 参与贡献

我们欢迎各种形式的贡献：
- 🐛 报告问题和建议改进
- 💡 提出新功能需求  
- 🔧 提交代码改进
- 📖 完善文档说明

请随时提交 Pull Request，让这个项目变得更好！
