namespace Linger.FileSystem.Remote;

/// <summary>
/// Provides common connection options for remote file systems.
/// </summary>
public class RemoteFileSystemOptions
{
    /// <summary>
    /// Gets or sets the remote server host name or IP address.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote server port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the user name used to authenticate with the remote server.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password used to authenticate with the remote server.
    /// </summary>
    /// <remarks>
    /// Avoid storing passwords in source code. Use a secret manager, environment variable,
    /// or protected configuration source in production.
    /// </remarks>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection timeout, in milliseconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the operation timeout, in milliseconds.
    /// </summary>
    public int OperationTimeout { get; set; } = 60000;
}
