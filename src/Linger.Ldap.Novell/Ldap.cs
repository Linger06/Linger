using System.Globalization;
using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Novell.Directory.Ldap;

namespace Linger.Ldap.Novell;

/// <summary>
/// LDAP client implementation using Novell.Directory.Ldap provider.
/// Provides cross-platform LDAP connectivity.
/// </summary>
public class Ldap : ILdap, IDisposable
{
    private readonly LdapConfig _ldapConfig;
    private readonly LdapConnection _ldapConn;
    private readonly ILogger<Ldap> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ldap"/> class.
    /// </summary>
    /// <param name="ldapConfig">The LDAP configuration.</param>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when ldapConfig is null.</exception>
    /// <exception cref="ArgumentException">Thrown when ldapConfig.Url is null or empty.</exception>
    public Ldap(LdapConfig ldapConfig, ILogger<Ldap>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(ldapConfig);

        if (ldapConfig.Url.IsNullOrEmpty())
        {
            throw new ArgumentException("Url is required for Novell LDAP provider. Unlike ActiveDirectory, automatic domain controller discovery is not available.", nameof(ldapConfig));
        }

        _ldapConfig = ldapConfig;
        _ldapConn = new LdapConnection { SecureSocketLayer = _ldapConfig.Security };
        _logger = logger ?? NullLogger<Ldap>.Instance;
    }

    public async Task<AdUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Finding user {UserName} in LDAP", userName);

        if (!await ConnectAsync(ldapCredentials).ConfigureAwait(false))
        {
            _logger.LogWarning("Failed to connect to LDAP server when finding user {UserName}", userName);
            return null;
        }

        try
        {
            var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
            var searchFilter = string.Format(CultureInfo.InvariantCulture, _ldapConfig.SearchFilter, userName);
            ILdapSearchResults? result = await _ldapConn.SearchAsync(effectiveSearchBase, LdapConnection.ScopeSub, searchFilter, null, false, cancellationToken).ConfigureAwait(false);

            LdapEntry? user = await result.NextAsync(cancellationToken).ConfigureAwait(false);
            if (user is null)
            {
                _logger.LogDebug("User {UserName} not found in LDAP", userName);
            }

            return user?.ToAdUser();
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task<IEnumerable<AdUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        if (!await ConnectAsync(ldapCredentials).ConfigureAwait(false)) return [];

        try
        {
            var ldapEntries = new List<LdapEntry>();

            // Use provided searchBase or fall back to config's SearchBase
            var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
            var searchFilter = string.Format(CultureInfo.InvariantCulture, _ldapConfig.SearchFilter, userName + "*");
            ILdapSearchResults? lsc = await _ldapConn.SearchAsync(effectiveSearchBase, LdapConnection.ScopeSub, searchFilter, null, false, cancellationToken).ConfigureAwait(false);
            while (await lsc.HasMoreAsync(cancellationToken).ConfigureAwait(false))
            {
                LdapEntry? nextEntry;
                try
                {
                    nextEntry = await lsc.NextAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (LdapException ex)
                {
                    _logger.LogWarning(ex, "Error retrieving LDAP entry while searching for user {UserName}, skipping entry", userName);
                    continue;
                }
                ldapEntries.Add(nextEntry);
            }
            return ldapEntries
                .Select(entry => entry.ToAdUser())
                .Where(user => user is not null)
                .Cast<AdUserInfo>()
                .ToList();
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task<(bool IsValid, AdUserInfo? AdUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating user {UserName} against LDAP", userName);

        var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
        var adUserInfo = await FindUserAsync(userName, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);

        if (adUserInfo is null) return (false, null);

        // Connect and bind directly with the user's DN + password to validate credentials,
        // avoiding unnecessary bind with config credentials.
        try
        {
            var port = _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
            await _ldapConn.ConnectAsync(_ldapConfig.Url, port, cancellationToken).ConfigureAwait(false);
            await _ldapConn.BindAsync(adUserInfo.Dn, password, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("User {UserName} validated successfully", userName);

            return (_ldapConn.Bound, adUserInfo);
        }
        catch (LdapException ex)
        {
            _logger.LogDebug(ex, "User {UserName} validation failed", userName);

            return (false, null);
        }
        finally
        {
            Disconnect();
        }
    }

    public bool IsConnected() => _ldapConn.Connected;

    public async Task<bool> ConnectAsync(LdapCredentials? ldapCredentials = null)
    {
        if (_ldapConn.Connected) return true;

        try
        {
            var port = _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
            _logger.LogDebug("Connecting to LDAP server {Url}:{Port}", _ldapConfig.Url, port);

            await _ldapConn.ConnectAsync(_ldapConfig.Url, port).ConfigureAwait(false);

            if (ldapCredentials is not null)
            {
                await BindCredentialsAsync(ldapCredentials).ConfigureAwait(false);
            }
            else if (_ldapConfig.Credentials is not null)
            {
                await BindCredentialsAsync(_ldapConfig.Credentials).ConfigureAwait(false);
            }

            _logger.LogDebug("Successfully connected to LDAP server {Url}", _ldapConfig.Url);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to LDAP server {Url}", _ldapConfig.Url);
            return false;
        }
    }

    private async Task BindCredentialsAsync(LdapCredentials credentials)
    {
        var domain = _ldapConfig.Domain;
        var userId = credentials.BindDn;
        var password = credentials.BindCredentials;

        if (userId.IsNotNullOrEmpty() && password.IsNotNullOrEmpty())
        {
            await _ldapConn.BindAsync($@"{domain}\{userId}", password).ConfigureAwait(false);
        }
        else
        {
            await _ldapConn.BindAsync(null, null).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Disconnects from the LDAP server.
    /// </summary>
    public void Disconnect()
    {
        if (_ldapConn.Connected)
        {
            _ldapConn.Disconnect();
        }
    }

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    public async Task<bool> UserExistsAsync(string userName, string? searchBase = null, CancellationToken cancellationToken = default) => await FindUserAsync(userName, searchBase: searchBase, cancellationToken: cancellationToken).ConfigureAwait(false) is not null;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Ldap"/> and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Ldap"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Disconnect();
            _ldapConn.Dispose();
        }

        _disposed = true;
    }
}
