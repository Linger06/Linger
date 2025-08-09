using System.Text;
using Linger.Extensions.Core;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Linger.Email;

public class Email : IEmail, IDisposable
{
    private readonly EmailConfig _emailConfig;
    private readonly SmtpClient _smtpClient;
    private Action<string>? _actionSendCompletedCallback;
    private bool _disposed;

    public Email(EmailConfig emailConfig)
    {
        _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
        _smtpClient = new SmtpClient();
        _smtpClient.MessageSent += SmtpClient_MessageSent;
    }

    public virtual async Task SendAsync(EmailMessage emailMessage, Action<string>? completedCallback = null)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);

        try
        {
            ValidateAndSetupEmailMessage(emailMessage);
            var mimeMessage = CreateMimeMessage(emailMessage);
            _actionSendCompletedCallback = completedCallback;

            await ConnectToServerAsync().ConfigureAwait(false);
            await SendMessageAsync(mimeMessage).ConfigureAwait(false);
        }
        finally
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
    }

    private void ValidateAndSetupEmailMessage(EmailMessage emailMessage)
    {
        if (emailMessage.From == null || emailMessage.From.Address.IsNullOrEmpty())
        {
            emailMessage.From = _emailConfig.From;
        }
        emailMessage.Bcc ??= _emailConfig.Bcc;
    }

    private async Task ConnectToServerAsync()
    {
        var secureOptions = DetermineSecureOptions();
        await _smtpClient.ConnectAsync(_emailConfig.Host, _emailConfig.Port, secureOptions).ConfigureAwait(false);

    if (_emailConfig.UserName.IsNotNullOrEmpty())
        {
            await _smtpClient.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password).ConfigureAwait(false);
        }
    }

    private SecureSocketOptions DetermineSecureOptions()
    {
        if (_emailConfig is { UseStartTls: false, UseSsl: false })
        {
            return SecureSocketOptions.None;
        }
        return _emailConfig.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
    }

    private async Task SendMessageAsync(MimeMessage message)
    {
        await _smtpClient.SendAsync(message).ConfigureAwait(false);
    }

    private async Task DisconnectAsync()
    {
        if (_smtpClient.IsConnected)
        {
            await _smtpClient.DisconnectAsync(true).ConfigureAwait(false);
        }
    }

    private void SmtpClient_MessageSent(object? sender, MessageSentEventArgs e)
    {
        _actionSendCompletedCallback?.Invoke(e.Response);
    }

    private static MimeMessage CreateMimeMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();
        AddMessageAddresses(message, emailMessage);
        AddMessageContent(message, emailMessage);
        SetMessageProperties(message, emailMessage);
        message.Prepare(EncodingConstraint.SevenBit);
        return message;
    }

    private static void AddMessageAddresses(MimeMessage message, EmailMessage emailMessage)
    {
        message.To.AddRange(emailMessage.To.Select(x => new MailboxAddress(x.Name, x.Address)));

        var fromAddress = emailMessage.From ??
            throw new InvalidOperationException("From address cannot be null");
        message.From.Add(new MailboxAddress(fromAddress.Name, fromAddress.Address));

        if (emailMessage.Bcc?.Count > 0)
        {
            message.Bcc.AddRange(emailMessage.Bcc.Select(x => new MailboxAddress(x.Name, x.Address)));
        }

        if (emailMessage.Cc?.Count > 0)
        {
            message.Cc.AddRange(emailMessage.Cc.Select(x => new MailboxAddress(x.Name, x.Address)));
        }
    }

    private static void AddMessageContent(MimeMessage message, EmailMessage emailMessage)
    {
        var bodyBuilder = new BodyBuilder();
        ProcessAttachments(emailMessage, bodyBuilder);
        SetMessageBody(bodyBuilder, emailMessage);
        message.Body = bodyBuilder.ToMessageBody();
    }

    private static void ProcessAttachments(EmailMessage emailMessage, BodyBuilder bodyBuilder)
    {
        if (emailMessage.AttachmentsPath?.Count > 0)
        {
            foreach (var path in emailMessage.AttachmentsPath)
            {
                var attachment = new AttachmentInfo
                {
                    FileName = Path.GetFileName(path),
                    Stream = File.OpenRead(path)
                };
                var mimePart = CreateMimePart(attachment);
                bodyBuilder.Attachments.Add(mimePart);
            }
        }

        var attachments = emailMessage.Attachments;

        if (attachments?.Count == 0 || attachments == null)
        {
            return;
        }
        foreach (var att in attachments)
        {
            var mimePart = CreateMimePart(att);
            bodyBuilder.Attachments.Add(mimePart);
        }
    }

    private static MimePart CreateMimePart(AttachmentInfo att)
    {
        var attachment = string.IsNullOrWhiteSpace(att.ContentType)
            ? new MimePart()
            : new MimePart(att.ContentType);

        attachment.Content = new MimeContent(att.Stream);
        attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
        attachment.ContentTransferEncoding = ContentEncoding.Base64;
        attachment.FileName = ConvertHeaderToBase64(att.FileName, Encoding.UTF8);

        return attachment;
    }

    private static void SetMessageBody(BodyBuilder bodyBuilder, EmailMessage emailMessage)
    {
        if (emailMessage.IsHtmlBody)
        {
            bodyBuilder.HtmlBody = emailMessage.Body;
        }
        else
        {
            bodyBuilder.TextBody = emailMessage.Body;
        }
    }

    private static void SetMessageProperties(MimeMessage message, EmailMessage emailMessage)
    {
        message.Subject = emailMessage.Subject;
        message.Priority = emailMessage.Priority;
        message.Importance = MessageImportance.Normal;
    }

    private static string ConvertHeaderToBase64(string inputStr, Encoding encoding)
    {
        if (string.IsNullOrEmpty(inputStr) || !inputStr.Any(c => c > 127))
        {
            return inputStr;
        }

        var base64String = Convert.ToBase64String(encoding.GetBytes(inputStr));
        return $"=?{encoding.WebName}?B?{base64String}?=";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _smtpClient.MessageSent -= SmtpClient_MessageSent;
            _smtpClient.Dispose();
        }

        _disposed = true;
    }

    public virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        if (_smtpClient.IsConnected)
        {
            await _smtpClient.DisconnectAsync(true).ConfigureAwait(false);
        }
        _smtpClient.MessageSent -= SmtpClient_MessageSent;
        _smtpClient.Dispose();
        _disposed = true;
    }
}
