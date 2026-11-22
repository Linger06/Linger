using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
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
public sealed class AdLdapClient : IActiveDirectoryClient, IActiveDirectoryCredentialValidator
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<AdLdapClient> _logger;

    // 惰性解析：域控自动发现是网络 I/O，不在构造函数中执行，避免拖慢或阻断 DI 容器启动
    private readonly Lazy<string> _url;

    private const string ObjectSid = "objectSid";
    private const string PrincipalName = "msDS-PrincipalName";
    private const string DnsHostName = "dnsHostName";
    private const string DefaultNamingContext = "defaultNamingContext";
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

    /// <inheritdoc />
    public bool ValidateCredentials(string userName, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);

        using var root = new DirectoryEntry(BuildLdapPath("RootDSE"), BuildBindUserName(userName), password,
            GetAuthenticationFlags(_ldapConfig.Security) & ~AuthenticationTypes.ServerBind);
        try
        {
            // DirectoryEntry 惰性绑定，不需要读取用户或域对象。
            _ = root.NativeObject;
            return true;
        }
        catch (COMException ex) when (ex.ErrorCode == unchecked((int)0x8007052E)) // ERROR_LOGON_FAILURE
        {
            return false;
        }
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
        var host = NormalizeServer(_url.Value);
        if (Uri.CheckHostName(host) != UriHostNameType.Dns)
        {
            return (false, null);
        }

        var flags = GetAuthenticationFlags(_ldapConfig.Security);
        var service = ResolveCredentials(null);
        var serviceName = BuildBindUserName(service?.BindDn);
        var serverPath = $"{LdapSchemePrefix}{host}/";
        try
        {
            using var root = new DirectoryEntry(serverPath + "RootDSE", serviceName, service?.BindCredentials, flags);
            root.RefreshCache([DnsHostName, DefaultNamingContext]);
            if (!IsPinnedController(host, root.Properties[DnsHostName].Value as string) ||
                root.Properties[DefaultNamingContext].Value is not string domainDn)
            {
                return (false, null);
            }

            using var domain = new DirectoryEntry(serverPath + domainDn, serviceName, service?.BindCredentials, flags);
            domain.RefreshCache([ObjectSid]);
            if (domain.Properties[ObjectSid].Value is not byte[] domainSid)
            {
                return (false, null);
            }

            var scope = searchBase ?? _ldapConfig.SearchBase;
            var path = serverPath + (string.IsNullOrWhiteSpace(scope) ? domainDn : scope);
            using var searchRoot = new DirectoryEntry(path, serviceName, service?.BindCredentials, flags);
            var login = BuildBindUserName(userName)!;
            var (attribute, value) = ParseIdentity(login);
            using var identitySearch = CreateIdentitySearcher(searchRoot, $"({attribute}={LdapHelper.EscapeFilterValue(value)})", [ObjectSid, PrincipalName]);
            using var matches = identitySearch.FindAll();
            if (matches.Count != 1 || matches[0].Properties[ObjectSid] is not { Count: 1 } sids || sids[0] is not byte[] sid ||
                !IsInControllerDomain(sid, domainSid) || matches[0].Properties[PrincipalName] is not { Count: 1 } names ||
                names[0] is not string canonical || !CanValidateIdentity(login, canonical))
            {
                return (false, null);
            }

            // ADSI 的显式安全绑定接受完整域账号；既不传裸名，也不尝试简单绑定。
            using var userRoot = new DirectoryEntry(serverPath + "RootDSE", canonical, password, flags);
            userRoot.RefreshCache([DnsHostName]);
            if (!IsPinnedController(host, userRoot.Properties[DnsHostName].Value as string))
            {
                return (false, null);
            }

            // 验密后仍在同一域控和 OU 内按 SID、账号取资料，防止重命名或同名账号串用。
            var sam = canonical.Substring(canonical.IndexOf('\\') + 1);
            var encodedSid = "\\" + BitConverter.ToString(sid).Replace("-", "\\");
            using var profileSearch = CreateIdentitySearcher(searchRoot,
                $"(&(objectSid={encodedSid})(sAMAccountName={LdapHelper.EscapeFilterValue(sam)}))", _ldapConfig.Attributes);
            using var profiles = profileSearch.FindAll();

            return profiles.Count == 1 ? (true, profiles.ToLdapUsersInfo()[0]) : (false, null);
        }
        catch (COMException ex)
        {
            _logger.LogDebug(ex, "Active Directory identity validation failed for {UserName}", userName);

            return (false, null);
        }
    }

    internal static AuthenticationTypes GetAuthenticationFlags(bool tls) => AuthenticationTypes.ServerBind | AuthenticationTypes.Secure |
        (tls ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.Signing | AuthenticationTypes.Sealing);

    internal static bool IsPinnedController(string host, string? dnsHostName) =>
        string.Equals(host, dnsHostName, StringComparison.OrdinalIgnoreCase);

    internal static bool IsInControllerDomain(byte[] sid, byte[] domainSid) =>
        new SecurityIdentifier(sid, 0).AccountDomainSid?.Equals(new SecurityIdentifier(domainSid, 0)) == true;

    internal static DirectorySearcher CreateIdentitySearcher(DirectoryEntry root, string identityFilter, string[]? attributes) => new(root,
        $"(&(objectCategory=person)(objectClass=user){identityFilter})", attributes, SearchScope.Subtree)
    {
        SizeLimit = 2,
        ReferralChasing = ReferralChasingOption.None,
        DerefAlias = DereferenceAlias.Never
    };

    internal static (string Attribute, string Value) ParseIdentity(string login)
    {
        if (login.Contains("="))
        {
            return (LdapUserType.Dn, login);
        }

        var separator = login.IndexOf('\\');
        if (separator >= 0)
        {
            return (LdapUserType.SamAccountName, login.Substring(separator + 1));
        }

        return login.Contains("@") ? (LdapUserType.Upn, login) : (LdapUserType.SamAccountName, login);
    }

    internal static bool CanValidateIdentity(string login, string? canonical)
    {
        var separator = canonical?.IndexOf('\\') ?? -1;
        if (separator <= 0 || separator == canonical!.Length - 1 || separator != canonical.LastIndexOf('\\'))
        {
            return false;
        }

        return ParseIdentity(login).Attribute != LdapUserType.SamAccountName ||
            string.Equals(login, login.Contains("\\") ? canonical : canonical.Substring(separator + 1), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Discovers the domain controller for the current domain.
    /// </summary>
    /// <returns>The domain controller hostname, e.g., "DC01.contoso.com"</returns>
    public static string GetDomainController()
    {
        var directoryContext = new DirectoryContext(DirectoryContextType.Domain);
        using var domainController = DomainController.FindOne(directoryContext);
        return domainController.Name;
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
        var normalizedServer = NormalizeServer(_url.Value);

        if (searchBase.IsNullOrWhiteSpace())
        {
            return $"{LdapSchemePrefix}{normalizedServer}";
        }

        return $"{LdapSchemePrefix}{normalizedServer}/{searchBase}";
    }

    private static string NormalizeServer(string server)
    {
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

        return normalizedServer.TrimEnd('/');
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
