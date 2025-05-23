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

## 使用示例

### 基本电子邮件发送
```csharp
// 简单文本邮件
var email = new EmailMessage 
{ 
    From = "sender@example.com", 
    To = new[] { "recipient@example.com" }, 
    Subject = "你好", 
    Body = "这是一封测试邮件" 
};
await emailService.SendAsync(email);

// 多个收件人
var groupEmail = new EmailMessage 
{ 
    From = "sender@example.com", 
    To = new[] { "recipient1@example.com", "recipient2@example.com" }, 
    Cc = new[] { "manager@example.com" }, 
    Bcc = new[] { "archive@example.com" }, 
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
    From = "reports@company.com", 
    To = new[] { "manager@company.com" },
    Subject = "月度报告", 
    IsHtml = true,
    Body = @"<h1>月度销售报告</h1><p>请查阅本月的附件报告。</p><p><strong>总销售额：</strong>¥50,000<br><strong>增长率：</strong>15%</p>" 
};
reportEmail.Attachments.Add(new EmailAttachment("monthly-report.pdf"));
await emailService.SendAsync(reportEmail);

// 带自定义名称的多个附件
var documentsEmail = new EmailMessage
{ 
    From = "documents@company.com", 
    To = new[] { "client@example.com" }, 
    Subject = "项目文档", 
    IsHtml = true, 
    Body = "请查阅附件中的项目文档。", 
    Attachments = new List { new EmailAttachment("specs.pdf", "项目规格说明.pdf"), new EmailAttachment("timeline.xlsx", "项目时间表.xlsx") }
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
