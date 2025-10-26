namespace Linger.Email.AspNetCore;

/// <summary>
/// Defines email service operations
/// </summary>
public interface IEmailService : IEmail
{
    /// <summary>
    /// Sends a plain text email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>A task representing the asynchronous send operation</returns>
    Task SendTextEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an HTML email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML email body</param>
    /// <returns>A task representing the asynchronous send operation</returns>
    Task SendHtmlEmailAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// Sends an email with attachments
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Indicates whether the body is HTML</param>
    /// <param name="attachmentPaths">Collection of file paths for attachments</param>
    /// <returns>A task representing the asynchronous send operation</returns>
    Task SendWithAttachmentsAsync(string to, string subject, string body, bool isHtml, IEnumerable<string> attachmentPaths);
}
