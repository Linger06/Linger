using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public sealed class AdLdapClient : IActiveDirectoryClient
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<AdLdapClient> _logger;

    // 惰性解析：域控自动发现是网络 I/O，不在构造函数中执行，避免拖慢或阻断 DI 容器启动
    private readonly Lazy<string> _url;

    private const string DefaultUserSearchFilterTemplate = "(&(objectCategory=person)(objectClass=user)(|(samAccountName={0})(userPrincipalName={0})(mail={0})(displayName={0})))";
    private const string LdapSchemePrefix = "LDAP://";
    private const string LdapsSchemePrefix = "LDAPS://";

    /// <summary>
    /// Initializes a client that discovers the current domain controller on first use.
    /// </summary>
    /// <remarks>
    /// Construction does not access the network. Use a configured constructor when credentials,
    /// a search base, a custom filter, LDAPS, or a result limit must be specified.
    /// </remarks>
    public AdLdapClient()
        : this(new LdapConfig
        {
            Url = string.Empty,
            Domain = string.Empty,
            SearchBase = string.Empty,
            SearchFilter = string.Empty
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdLdapClient"/> class.
    /// If <see cref="LdapConfig.Url"/> is not configured, the domain controller will be automatically
    /// discovered on first use (not during construction).
    /// </summary>
    /// <param name="ldapConfig">The LDAP configuration.</param>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when ldapConfig is null.</exception>
    public AdLdapClient(LdapConfig ldapConfig, ILogger<AdLdapClient>? logger = null)
    {
        var configSnapshot = new LdapConfig(ldapConfig);
        LdapHelper.ValidateConfig(configSnapshot);
        _logger = logger ?? NullLogger<AdLdapClient>.Instance;
        _ldapConfig = configSnapshot;

        _url = new Lazy<string>(
            () =>
            {
                if (!_ldapConfig.Url.IsNullOrEmpty())
                {
                    return _ldapConfig.Url;
                }

                _logger.LogInformation("LDAP Url not configured, attempting automatic domain controller discovery");
                var domainController = GetDomainController();
                _logger.LogInformation("Discovered domain controller: {DomainController}", domainController);
                return domainController;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Initializes a new instance from configured options.
    /// </summary>
    /// <param name="ldapOptions">The LDAP options.</param>
    /// <param name="logger">Optional logger instance.</param>
    public AdLdapClient(IOptions<LdapConfig> ldapOptions, ILogger<AdLdapClient>? logger = null)
        : this(ldapOptions?.Value ?? throw new ArgumentNullException(nameof(ldapOptions)), logger)
    {
    }

    /// <summary>
    /// Gets a certain user on Active Directory
    /// </summary>
    /// <param name="userName">The username to get</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Returns the user information if found; otherwise, null</returns>
    public LdapUserInfo? FindUser(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        _logger.LogDebug("Finding user {UserName} in Active Directory", userName);

        var searchFilter = BuildUserSearchFilter(userName, exactMatch: true);
        var users = SearchUsersByFilter(searchFilter, ldapCredentials, searchBase);

        var adUserInfo = users.FirstOrDefault();
        if (adUserInfo is null)
        {
            _logger.LogDebug("User {UserName} not found in Active Directory", userName);
            return null;
        }

        return adUserInfo;
    }

    /// <summary>
    /// Gets the base principal context
    /// </summary>
    /// <param name="ldapCredentials">Optional credentials for authentication</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Returns the PrincipalContext object</returns>
    private PrincipalContext GetPrincipalContext(LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ldapCredentials = ResolveCredentials(ldapCredentials);

        var configuredSearchBase = searchBase ?? _ldapConfig.SearchBase;
        var effectiveSearchBase = configuredSearchBase.IsNullOrWhiteSpace() ? null : configuredSearchBase;

        // Negotiate（Kerberos/NTLM）避免 SimpleBind 的明文密码传输；
        // Signing/Sealing 与 SecureSocketLayer 互斥，仅在非 SSL 时启用。
        var contextOptions = _ldapConfig.Security
            ? ContextOptions.Negotiate | ContextOptions.SecureSocketLayer
            : ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing;

        if (ldapCredentials is null)
        {
            return new PrincipalContext(ContextType.Domain, _url.Value, effectiveSearchBase, contextOptions);
        }

        return new PrincipalContext(ContextType.Domain, _url.Value, effectiveSearchBase, contextOptions,
            BuildBindUserName(ldapCredentials.BindDn), ldapCredentials.BindCredentials);
    }

    /// <summary>
    /// Falls back to the configured credentials when none are supplied explicitly.
    /// </summary>
    private LdapCredentials? ResolveCredentials(LdapCredentials? ldapCredentials)
    {
        LdapHelper.ValidateCredentials(ldapCredentials);
        if (ldapCredentials is not null)
        {
            return ldapCredentials;
        }

        var configured = _ldapConfig.Credentials;
        if (configured is not null && configured.BindDn.IsNotNullOrEmpty() && configured.BindCredentials.IsNotNullOrEmpty())
        {
            return configured;
        }

        return null;
    }

    /// <summary>
    /// Gets users by keyword or identity.
    /// </summary>
    /// <param name="userName">Keyword or identity to search (e.g., account, UPN, email, display name)</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Collection of matching users</returns>
    public IReadOnlyList<LdapUserInfo> GetUsers(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var searchFilter = BuildUserSearchFilter(userName, exactMatch: false);
        return SearchUsersByFilter(searchFilter, ldapCredentials, searchBase);
    }

    /// <summary>
    /// Searches users by raw LDAP filter with provider-agnostic contract.
    /// </summary>
    /// <param name="filter">Raw LDAP filter expression</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Collection of matching users</returns>
    public IReadOnlyList<LdapUserInfo> SearchUsersByFilter(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        ldapCredentials = ResolveCredentials(ldapCredentials);
        using var directoryEntry = CreateDirectoryEntry(ldapCredentials, searchBase);
        using var directorySearcher = new DirectorySearcher(directoryEntry)
        {
            SearchScope = SearchScope.Subtree,
            PageSize = Math.Min(1000, _ldapConfig.MaxResults),
            SizeLimit = _ldapConfig.MaxResults,
            Filter = filter
        };

        if (_ldapConfig.Attributes.IsNotNull())
        {
            foreach (var property in _ldapConfig.Attributes)
            {
                directorySearcher.PropertiesToLoad.Add(property);
            }
        }

        using var collection = directorySearcher.FindAll();
        IReadOnlyList<LdapUserInfo> users = collection.ToLdapUsersInfo();
        return users;
    }

    /// <summary>
    /// Validates the username and password of a given user
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password of the username to validate</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Returns True of user is valid</returns>
    public (bool IsValid, LdapUserInfo? LdapUserInfo) ValidateUser(string userName, string password, string? searchBase = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);

        _logger.LogDebug("Validating user {UserName} against Active Directory", userName);
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials: null, searchBase: searchBase);
        var result = principalContext.ValidateCredentials(BuildBindUserName(userName), password);
        if (result)
        {
            _logger.LogDebug("User {UserName} validated successfully", userName);

            try
            {
                var adUserInfo = FindUser(userName, searchBase: searchBase);

                return (true, adUserInfo);
            }
            catch (DirectoryServicesCOMException ex)
            {
                _logger.LogWarning(ex, "User {UserName} authenticated, but Active Directory user information could not be retrieved", userName);

                return (true, null);
            }
        }

        _logger.LogDebug("User {UserName} validation failed", userName);
        return (false, null);
    }

    /// <summary>
    /// Discovers the domain controller for the current domain.
    /// </summary>
    /// <returns>The domain controller hostname, e.g., "DC01.contoso.com"</returns>
    public static string GetDomainController()
    {
        var directoryContext = new DirectoryContext(DirectoryContextType.Domain);
        using var domainController = DomainController.FindOne(directoryContext);
        return domainController.ToString();
    }

    private DirectoryEntry CreateDirectoryEntry(LdapCredentials? ldapCredentials, string? searchBase = null)
    {
        var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
        var ldapPath = BuildLdapPath(effectiveSearchBase);
        var authenticationTypes = AuthenticationTypes.Secure;

        if (_ldapConfig.Security)
        {
            authenticationTypes |= AuthenticationTypes.SecureSocketsLayer;
        }

        if (ldapCredentials is null)
        {
            return new DirectoryEntry(ldapPath, null, null, authenticationTypes);
        }

        return new DirectoryEntry(ldapPath, BuildBindUserName(ldapCredentials.BindDn), ldapCredentials.BindCredentials, authenticationTypes);
    }

    private string BuildLdapPath(string? searchBase)
    {
        // ADSI 只识别 "LDAP://" 提供程序前缀（没有 "LDAPS:" 提供程序）；
        // SSL 由 AuthenticationTypes.SecureSocketsLayer 启用，而不是通过路径前缀。
        var server = _url.Value;
        var normalizedServer = server.IsNullOrWhiteSpace()
            ? string.Empty
            : server.Trim();

        if (normalizedServer.StartsWith(LdapSchemePrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalizedServer = normalizedServer.Substring(LdapSchemePrefix.Length);
        }
        else if (normalizedServer.StartsWith(LdapsSchemePrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalizedServer = normalizedServer.Substring(LdapsSchemePrefix.Length);
        }

        normalizedServer = normalizedServer.TrimEnd('/');

        if (searchBase.IsNullOrWhiteSpace())
        {
            return $"{LdapSchemePrefix}{normalizedServer}";
        }

        return $"{LdapSchemePrefix}{normalizedServer}/{searchBase}";
    }

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

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>True if user exists; otherwise, false</returns>
    public bool UserExists(string userName, string? searchBase = null) => FindUser(userName, searchBase: searchBase) is not null;
}
