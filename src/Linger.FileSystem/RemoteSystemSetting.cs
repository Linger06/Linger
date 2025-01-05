using System.Text;

namespace Linger.FileSystem;

public class RemoteSystemSetting
{
    public string Type { get; set; } = "Ftp";

#if NET7_0_OR_GREATER
    public required string Host { get; set; }
#else
    public string Host { get; set; } = default!;
#endif
    public int Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? AbsoluteRootDirectory { get; set; }

    public Encoding? Encoding { get; set; }
}