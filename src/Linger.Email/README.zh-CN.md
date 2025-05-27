# Linger.Email

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

一个 C# 电子邮件辅助库，为 .NET Framework 4.6.2+ 和 .NET Standard 2.0+ 提供简化的电子邮件操作和 SMTP 支持。

## 简介

Linger.Email 通过提供易于使用的接口来简化 .NET 应用程序中的电子邮件操作，包括发送电子邮件、管理附件和处理 SMTP 配置。

## 功能特性

### 电子邮件操作
- 使用流畅 API 简单发送电子邮件
- 支持 HTML 和纯文本格式
- 文件附件处理
- 电子邮件模板支持
- 异步操作

### SMTP 配置
- 支持多个 SMTP 服务器
- SSL/TLS 加密
- 自定义端口配置
- 身份验证选项

## 初始化与配置

在使用 Linger.Email 之前，您需要配置电子邮件客户端。有两种主要的方式：

### 直接实例化（适用于非 ASP.NET Core 项目）

```csharp
// 创建电子邮件配置
var emailConfig = new EmailConfig
{
    Host = "smtp.example.com",
    Port = 587,
    UseSsl = true,
    UserName = "username",
    Password = "password",
    From = new EmailAddress { Address = "noreply@example.com", Name = "Example System" }
};

// 创建电子邮件服务
var emailService = new Email(emailConfig);

// 现在可以使用 emailService 发送邮件
```

### 依赖注入（适用于 ASP.NET Core 项目）

对于 ASP.NET Core 项目，请使用 [Linger.Email.AspNetCore](../Linger.Email.AspNetCore) 包来简化电子邮件服务的配置和依赖注入。详细用法请参考该包的文档。

## 使用示例

### 基本电子邮件发送
```csharp
// 简单文本邮件
var email = new EmailMessage 
{ 
    From = new EmailAddress { Address = "sender@example.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "recipient@example.com" } }, 
    Subject = "你好", 
    Body = "这是一封测试邮件" 
};
await emailService.SendAsync(email);

// 多个收件人
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
    Subject = "团队会议", 
    Body = "明天下午2点开会" 
};
await emailService.SendAsync(groupEmail);
```

### 带附件的HTML电子邮件
```csharp
// 带单个附件的HTML邮件
var reportEmail = new EmailMessage 
{ 
    From = new EmailAddress { Address = "reports@company.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "manager@company.com" } },
    Subject = "月度报告", 
    IsHtmlBody = true,
    Body = @"<h1>月度销售报告</h1><p>请查阅本月的附件报告。</p><p><strong>总销售额：</strong>¥50,000<br><strong>增长率：</strong>15%</p>" 
};
// 添加文件路径作为附件
reportEmail.AttachmentsPath = new List<string> { "monthly-report.pdf" };
await emailService.SendAsync(reportEmail);

// 带自定义名称的多个附件
var documentsEmail = new EmailMessage
{ 
    From = new EmailAddress { Address = "documents@company.com" }, 
    To = new List<EmailAddress> { new EmailAddress { Address = "client@example.com" } }, 
    Subject = "项目文档", 
    IsHtmlBody = true, 
    Body = "请查阅附件中的项目文档。"
};
// 添加附件信息
documentsEmail.Attachments = new List<AttachmentInfo> { 
    new AttachmentInfo { FileName = "项目规格说明.pdf", Stream = File.OpenRead("specs.pdf") },
    new AttachmentInfo { FileName = "项目时间表.xlsx", Stream = File.OpenRead("timeline.xlsx") }
};
await emailService.SendAsync(documentsEmail);
```

## 安装

### 通过 Visual Studio

1. 打开`解决方案资源管理器`
2. 右键点击您的项目
3. 点击`管理 NuGet 包...`
4. 点击`浏览`选项卡，搜索 "Linger.Email"
5. 点击 `Linger.Email` 包，选择适当的版本并点击安装

### 通过 Package Manager 控制台

```
PM> Install-Package Linger.Email
```

### 通过 .NET CLI 命令行

```
dotnet add package Linger.Email
```
