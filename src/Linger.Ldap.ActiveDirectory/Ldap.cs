using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
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
public class Ldap : ILdap
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<Ldap> _logger;
    private readonly string _url;

    private const string DefaultUserSearchFilterTemplate = "(&(objectCategory=person)(objectClass=user)(|(samAccountName={0})(userPrincipalName={0})(mail={0})(displayName={0})))";
    private const string DefaultDirectorySearcherFilter = "(&(objectCategory=person)(objectClass=user))";
    private const string LdapSchemePrefix = "LDAP://";
    private const string LdapsSchemePrefix = "LDAPS://";

    /// <summary>
    /// Initializes a new instance of the <see cref="Ldap"/> class with inferred defaults.
    /// </summary>
    /// <remarks>
    /// Missing values are auto-filled, including <see cref="LdapConfig.Domain"/> and <see cref="LdapConfig.SearchBase"/>.
    /// </remarks>
    public Ldap()
        : this(BuildConfigWithDefaults(), logger: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ldap"/> class with inferred defaults and a custom logger.
    /// </summary>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <remarks>
    /// Missing values are auto-filled, including <see cref="LdapConfig.Domain"/> and <see cref="LdapConfig.SearchBase"/>.
    /// </remarks>
    public Ldap(ILogger<Ldap>? logger)
        : this(BuildConfigWithDefaults(), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ldap"/> class.
    /// If <see cref="LdapConfig.Url"/> is not configured, the domain controller will be automatically discovered.
    /// </summary>
    /// <param name="ldapConfig">The LDAP configuration.</param>
    /// <param name="logger">Optional logger instance. If null, <see cref="NullLogger{T}"/> is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when ldapConfig is null.</exception>
    public Ldap(LdapConfig ldapConfig, ILogger<Ldap>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(ldapConfig);
        _logger = logger ?? NullLogger<Ldap>.Instance;
        _ldapConfig = ldapConfig;

        // 如果 Url 为空，自动发现域控制器，只在构造时执行一次
        if (ldapConfig.Url.IsNullOrEmpty())
        {
            _logger.LogInformation("LDAP Url not configured, attempting automatic domain controller discovery");
            _url = GetDomainController();
            _logger.LogInformation("Discovered domain controller: {DomainController}", _url);
        }
        else
        {
            _url = ldapConfig.Url;
        }
    }

    /// <summary>
    /// Gets a certain user on Active Directory
    /// </summary>
    /// <param name="userName">The username to get</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns the user information if found; otherwise, null</returns>
    public async Task<AdUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
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
        if (ldapCredentials is null)
        {
            if (_ldapConfig.Credentials.IsNotNull() && _ldapConfig.Credentials.BindDn.IsNotNullOrEmpty() && _ldapConfig.Credentials.BindCredentials.IsNotNullOrEmpty())
            {
                ldapCredentials = _ldapConfig.Credentials;
            }
        }

        var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
        var contextOptions = ContextOptions.SimpleBind;

        if (_ldapConfig.Security)
        {
            contextOptions |= ContextOptions.SecureSocketLayer;
        }

        if (ldapCredentials is null)
        {
            return new PrincipalContext(ContextType.Domain, _url, effectiveSearchBase, contextOptions);
        }

        return new PrincipalContext(ContextType.Domain, _url, effectiveSearchBase, contextOptions,
            BuildBindUserName(ldapCredentials.BindDn), ldapCredentials.BindCredentials);
    }

    /// <summary>
    /// Gets all users matching the specified username pattern
    /// </summary>
    /// <param name="userName">Username pattern to search for</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching users</returns>
    public async Task<IEnumerable<AdUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
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
    public async Task<IEnumerable<AdUserInfo>> SearchUsersByFilterAsync(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        using var collection = SearchUsersByFilter(filter, ldapCredentials, searchBase);
        return await Task.Run(collection.ToAdUsersInfo, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the username and password of a given user
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password of the username to validate</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns True of user is valid</returns>
    public async Task<(bool IsValid, AdUserInfo? AdUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating user {UserName} against Active Directory", userName);
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials: null, searchBase: searchBase);
        var result = principalContext.ValidateCredentials(userName, password);
        if (result)
        {
            _logger.LogDebug("User {UserName} validated successfully", userName);
            var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
            var adUserInfo = await FindUserAsync(userName, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);
            return (true, adUserInfo);
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
        var domainController = DomainController.FindOne(directoryContext);
        return domainController.ToString();
    }

    /// <summary>
    /// Searches for users matching the specified LDAP filter.
    /// </summary>
    /// <param name="filter">LDAP filter string for user search</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Collection of search results matching the filter</returns>
    public SearchResultCollection SearchUsersByFilter(string? filter, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        if (ldapCredentials is null)
        {
            if (_ldapConfig.Credentials.IsNotNull() && _ldapConfig.Credentials.BindDn.IsNotNullOrEmpty() && _ldapConfig.Credentials.BindCredentials.IsNotNullOrEmpty())
            {
                ldapCredentials = _ldapConfig.Credentials;
            }
        }

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
        directorySearcher.Filter = filter.IsNullOrWhiteSpace()
            ? DefaultDirectorySearcherFilter
            : filter;

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
        var protocolPrefix = _ldapConfig.Security ? LdapsSchemePrefix : LdapSchemePrefix;
        var normalizedServer = _url.IsNullOrWhiteSpace()
            ? string.Empty
            : _url.Trim();

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
            return $"{protocolPrefix}{normalizedServer}";
        }

        return $"{protocolPrefix}{normalizedServer}/{searchBase}";
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
        var normalizedUserName = userName ?? string.Empty;

        if (!exactMatch)
        {
            if (normalizedUserName.IsNullOrWhiteSpace())
            {
                normalizedUserName = "*";
            }
            else if (normalizedUserName.IndexOf('*') < 0)
            {
                normalizedUserName += "*";
            }
        }

        var escapedUserName = EscapeLdapFilterValue(normalizedUserName, preserveAsterisk: !exactMatch);
        return BuildConfiguredSearchFilter(escapedUserName);
    }

    private string BuildConfiguredSearchFilter(string escapedUserName)
    {
        if (_ldapConfig.SearchFilter.IsNullOrWhiteSpace())
        {
            return string.Format(CultureInfo.InvariantCulture, DefaultUserSearchFilterTemplate, escapedUserName);
        }

        try
        {
            if (_ldapConfig.SearchFilter.Contains("{0}"))
            {
                return string.Format(CultureInfo.InvariantCulture, _ldapConfig.SearchFilter, escapedUserName);
            }

            return _ldapConfig.SearchFilter;
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex,
                "Invalid LDAP SearchFilter format: {SearchFilter}. Falling back to default filter.",
                _ldapConfig.SearchFilter);
            return string.Format(CultureInfo.InvariantCulture, DefaultUserSearchFilterTemplate, escapedUserName);
        }
    }

    private string? BuildBindUserName(string? bindDn)
    {
        if (bindDn.IsNullOrWhiteSpace())
        {
            return bindDn;
        }

        if (bindDn.Contains('\\') || bindDn.Contains('@') || bindDn.Contains('='))
        {
            return bindDn;
        }

        if (_ldapConfig.Domain.IsNullOrWhiteSpace())
        {
            return bindDn;
        }

        return $@"{_ldapConfig.Domain}\{bindDn}";
    }

    private static string EscapeLdapFilterValue(string value, bool preserveAsterisk)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escapedValue = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    escapedValue.Append("\\5c");
                    break;
                case '*':
                    escapedValue.Append(preserveAsterisk ? "*" : "\\2a");
                    break;
                case '(':
                    escapedValue.Append("\\28");
                    break;
                case ')':
                    escapedValue.Append("\\29");
                    break;
                case '\0':
                    escapedValue.Append("\\00");
                    break;
                default:
                    escapedValue.Append(character);
                    break;
            }
        }

        return escapedValue.ToString();
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
