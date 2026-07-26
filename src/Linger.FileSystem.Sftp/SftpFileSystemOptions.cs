using Linger.FileSystem.Remote;

namespace Linger.FileSystem.Sftp;

/// <summary>
/// Provides configuration options for <see cref="SftpFileSystem"/>.
/// </summary>
public class SftpFileSystemOptions : RemoteFileSystemOptions
{
    /// <summary>
    /// Gets or sets the path to the private key used for certificate authentication.
    /// When omitted, password authentication is used.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the optional private-key passphrase.
    /// </summary>
    public string? CertificatePassphrase { get; set; }
}
