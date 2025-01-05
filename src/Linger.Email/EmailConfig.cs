namespace Linger.Email;

public class EmailConfig
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public bool UseStartTls { get; set; }
    public EmailAddress From { get; set; } = null!;
    public List<EmailAddress>? Bcc { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}