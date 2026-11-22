using System.Text;
using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace Linger.Ldap.Novell;

/// <summary>
/// LDAP client implementation using Novell.Directory.Ldap provider.
/// Provides cross-platform LDAP connectivity.
/// </summary>
public sealed class NovellLdapClient : ILdapClient, ILdapCredentialValidator
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<NovellLdapClient> _logger;

    private const string PrincipalNameAttribute = "msDS-PrincipalName";
    private static readonly UTF8Encoding s_strictUtf8 = new(false, true);
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
        var configSnapshot = new LdapConfig(ldapConfig);
        LdapHelper.ValidateConfig(configSnapshot);

        if (configSnapshot.Url.IsNullOrEmpty())
        {
            throw new ArgumentException("Url is required for Novell LDAP provider. Unlike ActiveDirectory, automatic domain controller discovery is not available.", nameof(ldapConfig));
        }

        if (!string.IsNullOrEmpty(configSnapshot.Credentials?.BindCredentials))
        {
            RequirePasswordTls(configSnapshot);
        }

        _ldapConfig = configSnapshot;
        _logger = logger ?? NullLogger<NovellLdapClient>.Instance;
    }

    /// <summary>
    /// Initializes a new instance from configured options.
    /// </summary>
    /// <param name="ldapOptions">The LDAP options.</param>
    /// <param name="logger">Optional logger instance.</param>
    public NovellLdapClient(IOptions<LdapConfig> ldapOptions, ILogger<NovellLdapClient>? logger = null)
        : this(ldapOptions?.Value ?? throw new ArgumentNullException(nameof(ldapOptions)), logger)
    {
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

    public async Task<IReadOnlyList<LdapUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
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
    public async Task<IReadOnlyList<LdapUserInfo>> SearchUsersByFilterAsync(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        using var ldapConnection = CreateConnection();
        await ConnectAsync(ldapConnection, ldapCredentials, cancellationToken).ConfigureAwait(false);

        try
        {
            var users = new List<LdapUserInfo>();

            // Use provided searchBase or fall back to config's SearchBase
            var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
            ILdapSearchResults? lsc = await ldapConnection.SearchAsync(effectiveSearchBase, LdapConnection.ScopeSub, filter, _ldapConfig.Attributes, false, cancellationToken).ConfigureAwait(false);
            while (users.Count < _ldapConfig.MaxResults && await lsc.HasMoreAsync(cancellationToken).ConfigureAwait(false))
            {
                var nextEntry = await lsc.NextAsync(cancellationToken).ConfigureAwait(false);

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

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        var result = await ValidateAsync(userName, password, false, null, cancellationToken).ConfigureAwait(false);

        return result.IsValid;
    }

    /// <inheritdoc />
    public Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default) =>
        ValidateAsync(userName, password, true, searchBase, cancellationToken);

    private async Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateAsync(string userName, string password, bool readProfile, string? searchBase, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);
        RequirePasswordTls(_ldapConfig);

        using var connection = CreateConnection();
        await connection.ConnectAsync(_ldapConfig.Url, LdapConnection.DefaultSslPort, cancellationToken).ConfigureAwait(false);
        try
        {
            return await AuthenticateAsync(connection, userName, password, readProfile, searchBase, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Disconnect(connection);
        }
    }

    internal async Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> AuthenticateAsync(ILdapConnection connection, string userName, string password, bool readProfile, string? searchBase, CancellationToken cancellationToken)
    {
        var constraints = new LdapConstraints { ReferralFollowing = false };
        try
        {
            await connection.BindAsync(BuildBindUserName(userName), password, constraints, cancellationToken).ConfigureAwait(false);
        }
        catch (LdapException ex) when (ex.ResultCode == LdapException.InvalidCredentials)
        {
            return (false, null);
        }

        if (!readProfile)
        {
            return (true, null);
        }

        // 登录名不一定是目录规范名，只信任当前绑定会话返回的授权标识。
        try
        {
            var response = await connection.ExtendedOperationAsync(new LdapWhoAmIOperation(), constraints, cancellationToken).ConfigureAwait(false);
            if (response.ResultCode != LdapException.Success || response.Value is not { Length: > 0 } bytes)
            {
                return (false, null);
            }

            var user = await ReadBoundProfileAsync(connection, s_strictUtf8.GetString(bytes), searchBase ?? _ldapConfig.SearchBase, cancellationToken).ConfigureAwait(false);

            return (user is not null, user);
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "Could not verify the profile of the bound LDAP identity");
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogWarning(ex, "Invalid LDAP authorization identity encoding");
        }

        return (false, null);
    }

    internal static string? GetIdentityFilter(string authorizationId)
    {
        if (authorizationId.StartsWith("dn:", StringComparison.Ordinal) && authorizationId.Length > 3 && LdapDn.IsValid(authorizationId.Substring(3)))
        {
            var dn = LdapHelper.EscapeFilterValue(authorizationId.Substring(3));
            return $"(|(entryDN={dn})(distinguishedName={dn}))";
        }

        if (authorizationId.StartsWith("u:", StringComparison.Ordinal))
        {
            var separator = authorizationId.IndexOf('\\');
            if (separator > 2 && separator < authorizationId.Length - 1 && separator == authorizationId.LastIndexOf('\\'))
            {
                return $"(sAMAccountName={LdapHelper.EscapeFilterValue(authorizationId.Substring(separator + 1))})";
            }
        }

        return null;
    }

    internal async Task<LdapUserInfo?> ReadBoundProfileAsync(ILdapConnection connection, string authorizationId, string searchBase, CancellationToken cancellationToken)
    {
        var filter = GetIdentityFilter(authorizationId);
        if (filter is null)
        {
            return null;
        }

        var byDn = authorizationId.StartsWith("dn:", StringComparison.Ordinal);
        var attributes = byDn ? _ldapConfig.Attributes : (_ldapConfig.Attributes is { Length: > 0 } projection ? projection : ["*"]).Append(PrincipalNameAttribute).ToArray();
        var constraints = new LdapSearchConstraints
        {
            ReferralFollowing = false,
            Dereference = LdapSearchConstraints.DerefNever,
            MaxResults = 2
        };
        var results = await connection.SearchAsync(searchBase, LdapConnection.ScopeSub, filter, attributes, false, constraints, cancellationToken).ConfigureAwait(false);
        if (!await results.HasMoreAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var entry = await results.NextAsync(cancellationToken).ConfigureAwait(false);
        if (await results.HasMoreAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (byDn)
        {
            return LdapDn.Equals(entry.Dn, authorizationId.Substring(3)) ? entry.ToLdapUserInfo() : null;
        }

        var names = entry.GetAttributeSet();
        return names.ContainsKey(PrincipalNameAttribute) && names.GetAttribute(PrincipalNameAttribute).StringValueArray is [var canonical] &&
            string.Equals(canonical, authorizationId.Substring(2), StringComparison.OrdinalIgnoreCase) ? entry.ToLdapUserInfo() : null;
    }

    private static void RequirePasswordTls(LdapConfig config)
    {
        if (!config.Security)
        {
            throw new InvalidOperationException("Novell LDAP password binds require LDAPS (Security = true).");
        }
    }

    private async Task ConnectAsync(
        LdapConnection ldapConnection,
        LdapCredentials? ldapCredentials,
        CancellationToken cancellationToken)
    {
        var credentials = ldapCredentials ?? _ldapConfig.Credentials;
        LdapHelper.ValidateCredentials(credentials);
        if (!string.IsNullOrEmpty(credentials?.BindCredentials))
        {
            RequirePasswordTls(_ldapConfig);
        }

        var port = _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
        _logger.LogDebug("Connecting to LDAP server {Url}:{Port}", _ldapConfig.Url, port);

        await ldapConnection.ConnectAsync(_ldapConfig.Url, port, cancellationToken).ConfigureAwait(false);

        if (credentials is not null)
        {
            await BindCredentialsAsync(ldapConnection, credentials, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug("Successfully connected to LDAP server {Url}", _ldapConfig.Url);
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
            await ldapConnection.BindAsync(bindUserName, password, new LdapConstraints { ReferralFollowing = false }, cancellationToken).ConfigureAwait(false);
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
