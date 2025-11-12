using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Linger.Email.AspNetCore;

/// <summary>
/// Provides email service functionality with logging support
/// </summary>
public sealed class EmailService(
    IOptions<EmailConfig> emailConfig,
    ILogger<EmailService> logger) : Email(emailConfig?.Value ?? throw new ArgumentNullException(nameof(emailConfig))), IEmailService
{
    private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private bool _disposed;

    /// <inheritdoc/>
    public override async Task SendAsync(EmailMessage emailMessage, Action<string>? completedCallback = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);

        try
        {
            _logger.LogDebug("Sending email to {Recipients}",
                string.Join(", ", emailMessage.To.Select(x => x.Address)));

            await base.SendAsync(emailMessage, response =>
            {
                _logger.LogDebug("Email sent successfully: {Response}", response);
                completedCallback?.Invoke(response);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email: {Error}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task SendTextEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = CreateEmailMessage(to, subject, body, false);
        return SendAsync(message, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = CreateEmailMessage(to, subject, htmlBody, true);
        return SendAsync(message, null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendWithAttachmentsAsync(string to, string subject, string body, bool isHtml, IEnumerable<string> attachmentPaths, CancellationToken cancellationToken = default)
    {
        var message = CreateEmailMessage(to, subject, body, isHtml);
        message.AttachmentsPath = attachmentPaths.ToList();
        return SendAsync(message, null, cancellationToken);
    }

    private static EmailMessage CreateEmailMessage(string to, string subject, string body, bool isHtml) =>
        new()
        {
            To = [new EmailAddress { Address = to }],
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
            await base.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disposing email service resources");
        }
        finally
        {
            _disposed = true;
        }
    }
}
