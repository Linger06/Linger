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
public sealed class NovellLdapClient : ILdapClient
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<NovellLdapClient> _logger;

    private const string DefaultUserSearchFilterTemplate = "(&(objectClass=person)(|(uid={0})(sAMAccountName={0})(userPrincipalName={0})(mail={0})(cn={0})(displayName={0})))";

    /// <summary>
    /// Initializes a new instance of the <see cref="NovellLdapClient"/> class.
    /// </summary>
    /// <param name="ldapConfig">The LDAP configuration.</param>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when ldapConfig is null.</exception>
    /// <exception cref="ArgumentException">Thrown when ldapConfig.Url is null or empty.</exception>
    public NovellLdapClient(LdapConfig ldapConfig, ILogger<NovellLdapClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(ldapConfig);

        if (ldapConfig.Url.IsNullOrEmpty())
        {
            throw new ArgumentException("Url is required for Novell LDAP provider. Unlike ActiveDirectory, automatic domain controller discovery is not available.", nameof(ldapConfig));
        }

        _ldapConfig = ldapConfig;
        _logger = logger ?? NullLogger<NovellLdapClient>.Instance;
    }

    public async Task<LdapUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        _logger.LogDebug("Finding user {UserName} in LDAP", userName);

        var searchFilter = BuildUserSearchFilter(userName, exactMatch: true);
        var users = await SearchUsersByFilterAsync(searchFilter, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);

        var user = users.FirstOrDefault();
        if (user is null)
        {
            _logger.LogDebug("User {UserName} not found in LDAP", userName);
        }

        return user;
    }

    public async Task<IEnumerable<LdapUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var searchFilter = BuildUserSearchFilter(userName, exactMatch: false);
        return await SearchUsersByFilterAsync(searchFilter, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches users by raw LDAP filter with provider-agnostic contract.
    /// </summary>
    /// <param name="filter">Raw LDAP filter expression</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching users</returns>
    public async Task<IEnumerable<LdapUserInfo>> SearchUsersByFilterAsync(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        using var ldapConnection = CreateConnection();
        if (!await ConnectAsync(ldapConnection, ldapCredentials, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("Failed to connect to LDAP server when searching users by filter");
            return [];
        }

        try
        {
            var users = new List<LdapUserInfo>();

            // Use provided searchBase or fall back to config's SearchBase
            var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
            ILdapSearchResults? lsc = await ldapConnection.SearchAsync(effectiveSearchBase, LdapConnection.ScopeSub, filter, _ldapConfig.Attributes, false, cancellationToken).ConfigureAwait(false);
            while (await lsc.HasMoreAsync(cancellationToken).ConfigureAwait(false))
            {
                LdapEntry? nextEntry;
                try
                {
                    nextEntry = await lsc.NextAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (LdapException ex)
                {
                    _logger.LogWarning(ex, "Error retrieving LDAP entry while searching users by filter {Filter}, skipping entry", filter);
                    continue;
                }

                if (nextEntry.ToLdapUserInfo() is { } user)
                {
                    users.Add(user);
                }
            }

            return users;
        }
        finally
        {
            Disconnect(ldapConnection);
        }
    }

    public async Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);

        _logger.LogDebug("Validating user {UserName} against LDAP", userName);

        using var ldapConnection = CreateConnection();
        try
        {
            var port = _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
            await ldapConnection.ConnectAsync(_ldapConfig.Url, port, cancellationToken).ConfigureAwait(false);
            await ldapConnection.BindAsync(BuildBindUserName(userName), password, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("User {UserName} validated successfully", userName);
        }
        catch (LdapException ex)
        {
            _logger.LogDebug(ex, "User {UserName} validation failed", userName);

            return (false, null);
        }
        finally
        {
            Disconnect(ldapConnection);
        }

        try
        {
            var adUserInfo = await FindUserAsync(
                userName,
                searchBase: searchBase,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return (true, adUserInfo);
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "User {UserName} authenticated, but LDAP user information could not be retrieved", userName);

            return (true, null);
        }
    }

    private async Task<bool> ConnectAsync(
        LdapConnection ldapConnection,
        LdapCredentials? ldapCredentials,
        CancellationToken cancellationToken)
    {
        try
        {
            var port = _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
            _logger.LogDebug("Connecting to LDAP server {Url}:{Port}", _ldapConfig.Url, port);

            await ldapConnection.ConnectAsync(_ldapConfig.Url, port, cancellationToken).ConfigureAwait(false);

            if (ldapCredentials is not null)
            {
                await BindCredentialsAsync(ldapConnection, ldapCredentials, cancellationToken).ConfigureAwait(false);
            }
            else if (_ldapConfig.Credentials is not null)
            {
                await BindCredentialsAsync(ldapConnection, _ldapConfig.Credentials, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Successfully connected to LDAP server {Url}", _ldapConfig.Url);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to LDAP server {Url}", _ldapConfig.Url);
            return false;
        }
    }

    private async Task BindCredentialsAsync(
        LdapConnection ldapConnection,
        LdapCredentials credentials,
        CancellationToken cancellationToken)
    {
        var userId = credentials.BindDn;
        var password = credentials.BindCredentials;

        if (userId.IsNotNullOrEmpty() && password.IsNotNullOrEmpty())
        {
            var bindUserName = BuildBindUserName(userId);
            await ldapConnection.BindAsync(bindUserName, password, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await ldapConnection.BindAsync(null, null, cancellationToken).ConfigureAwait(false);
        }
    }

    private LdapConnection CreateConnection() => new()
    {
        SecureSocketLayer = _ldapConfig.Security
    };

    private string BuildUserSearchFilter(string userName, bool exactMatch)
    {
        var filter = LdapHelper.BuildUserSearchFilter(userName, exactMatch, _ldapConfig.SearchFilter, DefaultUserSearchFilterTemplate, out var usedFallback);
        if (usedFallback)
        {
            _logger.LogWarning(
                "Invalid LDAP SearchFilter format: {SearchFilter}. Falling back to default filter.",
                _ldapConfig.SearchFilter);
        }

        return filter;
    }

    private string? BuildBindUserName(string? bindDn) => LdapHelper.BuildBindUserName(bindDn, _ldapConfig.Domain);

    private static void Disconnect(LdapConnection ldapConnection)
    {
        if (ldapConnection.Connected)
        {
            ldapConnection.Disconnect();
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

}
