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

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class Ldap : ILdap
{
    private readonly LdapConfig _ldapConfig;
    private readonly ILogger<Ldap> _logger;
    private readonly string _url;

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
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials, searchBase);
        using var userPrincipal = UserPrincipal.FindByIdentity(principalContext, userName);
        if (userPrincipal is null)
        {
            _logger.LogDebug("User {UserName} not found in Active Directory", userName);

            return null;
        }

        return await Task.Run(userPrincipal.ToAdUser, cancellationToken).ConfigureAwait(false);
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
            Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={username}))",
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

        if (ldapCredentials is null)
        {
            return new PrincipalContext(ContextType.Domain, _url, effectiveSearchBase, ContextOptions.SimpleBind);
        }

        return new PrincipalContext(ContextType.Domain, _url, effectiveSearchBase, ContextOptions.SimpleBind,
            $@"{_ldapConfig.Domain}\{ldapCredentials.BindDn}", ldapCredentials.BindCredentials);
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
        using var collection = SearchUsersByFilter($"(samAccountName={userName}*)(userPrincipalName={userName}*)(mail={userName}*)(displayName={userName}*)", ldapCredentials, searchBase);

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
        if (string.IsNullOrEmpty(filter))
        {
            directorySearcher.Filter = "(&(objectCategory=person)(objectClass=user))";
        }
        else
        {
            directorySearcher.Filter = "(&(objectCategory=person)(objectClass=user)(|" + filter + "))";
        }

        var userCollection = directorySearcher.FindAll();
        return userCollection;
    }

    private DirectoryEntry CreateDirectoryEntry(LdapCredentials? ldapCredentials, string? searchBase = null)
    {
        var effectiveSearchBase = searchBase ?? _ldapConfig.SearchBase;
        var ldapPath = $"LDAP://{_url}/{effectiveSearchBase}";

        if (ldapCredentials is null)
        {
            return new DirectoryEntry(ldapPath);
        }

        return new DirectoryEntry(ldapPath, $@"{_ldapConfig.Domain}\{ldapCredentials.BindDn}", ldapCredentials.BindCredentials);
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
