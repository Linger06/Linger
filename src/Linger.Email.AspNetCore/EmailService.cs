using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Linger.Email.AspNetCore;

/// <summary>
/// 邮件服务实现
/// </summary>
/// <remarks>
/// 初始化邮件服务
/// </remarks>
public sealed class EmailService(
    IOptions<EmailConfig> emailConfig,
    ILogger<EmailService> logger) : Email(emailConfig?.Value ?? throw new ArgumentNullException(nameof(emailConfig))), IEmailService
{
    private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private bool _disposed;

    /// <inheritdoc/>
    public override async Task SendAsync(EmailMessage emailMessage, Action<string>? completedCallback = null)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);

        try
        {
            _logger.LogInformation("正在发送邮件到 {Recipients}",
                string.Join(", ", emailMessage.To.Select(x => x.Address)));

            await base.SendAsync(emailMessage, response =>
            {
                _logger.LogInformation("邮件发送成功: {Response}", response);
                completedCallback?.Invoke(response);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送邮件失败: {Error}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task SendTextEmailAsync(string to, string subject, string body)
    {
        var message = CreateEmailMessage(to, subject, body, false);
        return SendAsync(message);
    }

    /// <inheritdoc/>
    public Task SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        var message = CreateEmailMessage(to, subject, htmlBody, true);
        return SendAsync(message);
    }

    /// <inheritdoc/>
    public Task SendWithAttachmentsAsync(string to, string subject, string body, bool isHtml, IEnumerable<string> attachmentPaths)
    {
        var message = CreateEmailMessage(to, subject, body, isHtml);
        message.AttachmentsPath = attachmentPaths.ToList();
        return SendAsync(message);
    }

    private static EmailMessage CreateEmailMessage(string to, string subject, string body, bool isHtml) =>
        new()
        {
            To = [new() { Address = to }],
            Subject = subject,
            Body = body,
            IsHtmlBody = isHtml
        };

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await base.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放邮件服务资源时发生错误");
        }
        finally
        {
            _disposed = true;
        }
    }
}
