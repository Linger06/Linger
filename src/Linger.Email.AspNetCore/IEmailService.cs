namespace Linger.Email.AspNetCore;

/// <summary>
/// 邮件服务接口
/// </summary>
public interface IEmailService : IEmail
{
    /// <summary>
    /// 发送文本邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">主题</param>
    /// <param name="body">内容</param>
    /// <returns>发送任务</returns>
    Task SendTextEmailAsync(string to, string subject, string body);

    /// <summary>
    /// 发送HTML邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">主题</param>
    /// <param name="htmlBody">HTML内容</param>
    /// <returns>发送任务</returns>
    Task SendHtmlEmailAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// 发送带附件的邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">主题</param>
    /// <param name="body">内容</param>
    /// <param name="isHtml">是否HTML内容</param>
    /// <param name="attachmentPaths">附件路径列表</param>
    /// <returns>发送任务</returns>
    Task SendWithAttachmentsAsync(string to, string subject, string body, bool isHtml, IEnumerable<string> attachmentPaths);
}
