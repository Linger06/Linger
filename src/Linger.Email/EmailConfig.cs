namespace Linger.Email;

public class EmailConfig
{
    public string? Host { get; set; }
    public int Port { get; set; }
    /// <summary>Require TLS from connection start unless <see cref="UseStartTls"/> is enabled. Defaults to false.</summary>
    public bool UseSsl { get; set; }
    /// <summary>Require a successful STARTTLS upgrade; takes precedence over <see cref="UseSsl"/>. Defaults to false.</summary>
    public bool UseStartTls { get; set; }
    public EmailAddress From { get; set; } = null!;
    public List<EmailAddress>? Bcc { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}