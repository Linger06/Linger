using System.Text;
using Linger.FileSystem.Remote;

namespace Linger.FileSystem.Ftp;

/// <summary>
/// Provides configuration options for <see cref="FtpFileSystem"/>.
/// </summary>
public class FtpFileSystemOptions : RemoteFileSystemOptions
{
    /// <summary>
    /// Gets or sets the encoding used by the FTP client. The default is UTF-8.
    /// </summary>
    public Encoding? Encoding { get; set; }
}
