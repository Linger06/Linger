using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class AdLdapClient : ILdapClient
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<AdLdapClient> _logger;

    // 惰性解析：域控自动发现是网络 I/O，不在构造函数中执行，避免拖慢或阻断 DI 容器启动
    private readonly Lazy<string> _url;

    private const string DefaultUserSearchFilterTemplate = "(&(objectCategory=person)(objectClass=user)(|(samAccountName={0})(userPrincipalName={0})(mail={0})(displayName={0})))";
    private const string LdapSchemePrefix = "LDAP://";
    private const string LdapsSchemePrefix = "LDAPS://";

    /// <summary>
    /// Initializes a new instance of the <see cref="AdLdapClient"/> class with inferred defaults.
    /// </summary>
    /// <remarks>
    /// Missing values are auto-filled, including <see cref="LdapConfig.Domain"/> and <see cref="LdapConfig.SearchBase"/>.
    /// </remarks>
    public AdLdapClient()
        : this(BuildConfigWithDefaults(), logger: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdLdapClient"/> class with inferred defaults and a custom logger.
    /// </summary>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <remarks>
    /// Missing values are auto-filled, including <see cref="LdapConfig.Domain"/> and <see cref="LdapConfig.SearchBase"/>.
    /// </remarks>
    public AdLdapClient(ILogger<AdLdapClient>? logger)
        : this(BuildConfigWithDefaults(), logger)
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
        ArgumentNullException.ThrowIfNull(ldapConfig);
        _logger = logger ?? NullLogger<AdLdapClient>.Instance;
        _ldapConfig = ldapConfig;

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
    /// Gets a certain user on Active Directory
    /// </summary>
    /// <param name="userName">The username to get</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns the user information if found; otherwise, null</returns>
    public async Task<LdapUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        _logger.LogDebug("Finding user {UserName} in Active Directory", userName);

        var searchFilter = BuildUserSearchFilter(userName, exactMatch: true);
        var users = await SearchUsersByFilterAsync(searchFilter, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);

        var adUserInfo = users.FirstOrDefault();
        if (adUserInfo is null)
        {
            _logger.LogDebug("User {UserName} not found in Active Directory", userName);
            return null;
        }

        return adUserInfo;
    }

    /// <summary>
    /// Gets the DirectoryEntry for a user by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The DirectoryEntry for the user.</returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="DirectoryEntry"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the user is not found.</exception>
    public DirectoryEntry GetEntryByUsername(string username)
    {
        using var directoryEntry = CreateDirectoryEntry(ldapCredentials: null);
        using var searcher = new DirectorySearcher(directoryEntry)
        {
            Filter = BuildUserSearchFilter(username, exactMatch: true),
            SearchScope = SearchScope.Subtree
        };

        var result = searcher.FindOne();
        if (result is null)
        {
            throw new InvalidOperationException($"User '{username}' not found in Active Directory.");
        }

        return result.GetDirectoryEntry();
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

        var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching users</returns>
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
    /// <returns>Collection of matching users</returns>
    public IEnumerable<LdapUserInfo> SearchUsersByFilter(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        using var collection = SearchUserEntriesByFilterCore(filter, ldapCredentials, searchBase);
        return collection.ToLdapUsersInfo();
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

        return await Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return SearchUsersByFilter(filter, ldapCredentials, searchBase);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the username and password of a given user
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password of the username to validate</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns True of user is valid</returns>
    public async Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Validating user {UserName} against Active Directory", userName);
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials: null, searchBase: searchBase);
        var result = principalContext.ValidateCredentials(BuildBindUserName(userName), password);
        cancellationToken.ThrowIfCancellationRequested();
        if (result)
        {
            _logger.LogDebug("User {UserName} validated successfully", userName);

            try
            {
                var adUserInfo = await FindUserAsync(
                    userName,
                    searchBase: searchBase,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

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

    private SearchResultCollection SearchUserEntriesByFilterCore(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        ldapCredentials = ResolveCredentials(ldapCredentials);

        using DirectoryEntry directoryEntry = CreateDirectoryEntry(ldapCredentials, searchBase);
        using DirectorySearcher directorySearcher = new(directoryEntry);
        directorySearcher.SearchScope = SearchScope.Subtree;
        directorySearcher.PageSize = 1000;

        if (_ldapConfig.Attributes.IsNotNull())
        {
            foreach (var property in _ldapConfig.Attributes)
            {
                directorySearcher.PropertiesToLoad.Add(property);
            }
        }

        // 构建搜索过滤器
        directorySearcher.Filter = filter;

        var userCollection = directorySearcher.FindAll();
        return userCollection;
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

    private static LdapConfig BuildConfigWithDefaults()
    {
        var normalizedDomain = ResolveDomain();
        var normalizedSearchBase = ResolveSearchBase(normalizedDomain);

        return new LdapConfig
        {
            Url = string.Empty,
            Security = false,
            Domain = normalizedDomain,
            Credentials = null,
            SearchBase = normalizedSearchBase,
            SearchFilter = string.Empty,
            Attributes = null
        };
    }

    private static string ResolveDomain()
    {
        try
        {
            var currentDomain = Domain.GetCurrentDomain().Name;
            if (!currentDomain.IsNullOrWhiteSpace())
            {
                return currentDomain.Trim();
            }
        }
        catch
        {
            // Ignore and fall back to empty domain.
        }

        return string.Empty;
    }

    private static string ResolveSearchBase(string domain)
    {
        var searchBaseFromDomain = BuildDistinguishedNameFromDomain(domain);
        if (!searchBaseFromDomain.IsNullOrWhiteSpace())
        {
            return searchBaseFromDomain;
        }

        try
        {
            using var directoryEntry = Domain.GetCurrentDomain().GetDirectoryEntry();
            var distinguishedName = directoryEntry.Properties["distinguishedName"]?.Value?.ToString();
            if (!distinguishedName.IsNullOrWhiteSpace())
            {
                return distinguishedName;
            }
        }
        catch
        {
            // Ignore and fall back to empty search base.
        }

        return string.Empty;
    }

    private static string BuildDistinguishedNameFromDomain(string domain)
    {
        if (domain.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        var normalizedDomain = domain.Trim();
        if (normalizedDomain.Contains("DC=", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedDomain;
        }

        if (!normalizedDomain.Contains('.'))
        {
            return string.Empty;
        }

        var domainParts = normalizedDomain.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (domainParts.Length == 0)
        {
            return string.Empty;
        }

        var distinguishedNameBuilder = new StringBuilder();
        for (var index = 0; index < domainParts.Length; index++)
        {
            if (index > 0)
            {
                distinguishedNameBuilder.Append(',');
            }

            distinguishedNameBuilder.Append("DC=");
            distinguishedNameBuilder.Append(domainParts[index].Trim());
        }

        return distinguishedNameBuilder.ToString();
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    public async Task<bool> UserExistsAsync(string userName, string? searchBase = null, CancellationToken cancellationToken = default) => await FindUserAsync(userName, searchBase: searchBase, cancellationToken: cancellationToken).ConfigureAwait(false) is not null;
}
