using Linger.FileSystem.Remote;

namespace Linger.FileSystem.Sftp;

/// <summary>
/// Provides configuration options for <see cref="SftpFileSystem"/>.
/// </summary>
public class SftpFileSystemOptions : RemoteFileSystemOptions
{
    /// <summary>
    /// Gets or sets the required server host-key fingerprint in OpenSSH format:
    /// <c>SHA256:</c> followed by the unpadded Base64 SHA-256 digest (43 characters).
    /// The default is <see langword="null"/>; a missing or invalid pin prevents client construction.
    /// </summary>
    public string? HostKeyFingerprintSha256 { get; set; }

    /// <summary>
    /// Gets or sets the path to the private key used for client authentication, not for server verification.
    /// When omitted, password authentication is used.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the optional private-key passphrase.
    /// </summary>
    public string? CertificatePassphrase { get; set; }
}
