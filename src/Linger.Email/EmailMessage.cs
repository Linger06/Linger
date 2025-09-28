using MimeKit;

namespace Linger.Email;

public class EmailMessage
{
    public EmailAddress? From { get; set; }
    public List<EmailAddress> To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public bool IsHtmlBody { get; set; }
    public string? Body { get; set; }
    public List<EmailAddress>? Cc { get; set; }
    public List<EmailAddress>? Bcc { get; set; }
    public MessagePriority Priority { get; set; }
    public List<string>? AttachmentsPath { get; set; }
    public List<AttachmentInfo>? Attachments { get; set; }
}
