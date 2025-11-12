using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using Linger.Extensions.Core;
using Linger.Helper;
using Linger.Ldap.Contracts;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class Ldap(LdapConfig ldapConfig) : ILdap
{
    /// <summary>
    /// Gets a certain user on Active Directory
    /// </summary>
    /// <param name="userName">The username to get</param>
    /// <param name="ldapCredentials"></param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns the UserPrincipal Object</returns>
    public async Task<AdUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials, searchBase);
        var userPrincipal = UserPrincipal.FindByIdentity(principalContext, userName);
        return await Task.Run(userPrincipal.ToAdUser, cancellationToken).ConfigureAwait(false);
    }

    public DirectoryEntry GetEntryByUsername(string username)
    {
        var ctx = new PrincipalContext(ContextType.Domain, ldapConfig.Url);
        var user = UserPrincipal.FindByIdentity(ctx, username);
        var entry = user.GetUnderlyingObject() as DirectoryEntry;
        entry.EnsureIsNotNull();
        return entry;
    }

    /// <summary>
    /// Gets the base principal context
    /// </summary>
    /// <param name="ldapCredentials">Optional credentials for authentication</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <returns>Returns the PrincipalContext object</returns>
    private PrincipalContext GetPrincipalContext(LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        if (ldapCredentials == null)
        {
            if (ldapConfig.Credentials.IsNotNull() && ldapConfig.Credentials.BindDn.IsNotNullOrEmpty() && ldapConfig.Credentials.BindCredentials.IsNotNullOrEmpty())
            {
                ldapCredentials = ldapConfig.Credentials;
            }
        }

        // Use provided searchBase or fall back to config's SearchBase
        var effectiveSearchBase = searchBase ?? ldapConfig.SearchBase;

        if (ldapCredentials == null)
        {
            var principalContext = new PrincipalContext(ContextType.Domain, ldapConfig.Url, effectiveSearchBase, ContextOptions.SimpleBind);
            return principalContext;
        }
        else
        {
            var domain = ldapConfig.Domain;
            var userId = ldapCredentials.BindDn;
            var password = ldapCredentials.BindCredentials;
            var principalContext = new PrincipalContext(ContextType.Domain, ldapConfig.Url, effectiveSearchBase, ContextOptions.SimpleBind,
            $@"{domain}\{userId}", password);
            return principalContext;
        }
    }

    public async Task<IEnumerable<AdUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default)
    {
        var collection = SearchUsersByFilter($"(samAccountName={userName}*)(userPrincipalName={userName}*)(mail={userName}*)(displayName={userName}*)", ldapCredentials, searchBase);
        return await Task.Run(collection.ToAdUsersInfo, cancellationToken).ConfigureAwait(false);
    }

    //private IEnumerable<AdUserInfo> GetUsers2(string userName, LdapCredentials? ldapCredentials = null)
    //{
    //    PrincipalContext principalContext = GetPrincipalContext(ldapCredentials);
    //    UserPrincipal userPrincipal = new UserPrincipal(principalContext);
    //    //userPrincipal.SamAccountName = userName + "*";

    //    PrincipalSearcher principalSearcher = new PrincipalSearcher(userPrincipal);

    //    var adUserList = new List<AdUserInfo>();
    //    var usersPrincipal = principalSearcher.FindAll().Cast<UserPrincipal>()
    //        .Where(x => x.SamAccountName.ToUpperInvariant().StartsWith(userName.ToUpperInvariant())
    //        || x.UserPrincipalName.ToUpperInvariant().StartsWith(userName.ToUpperInvariant())
    //        || x.EmailAddress.Contains(userName, StringComparison.OrdinalIgnoreCase)
    //        || x.DisplayName.Contains(userName, StringComparison.OrdinalIgnoreCase)
    //        || x.DistinguishedName.ToUpperInvariant().StartsWith(userName.ToUpperInvariant())
    //        );

    //    foreach (UserPrincipal userSearchResult in usersPrincipal)
    //    {
    //        var adUserInfo = userSearchResult.ToAdUser();
    //        adUserList.Add(adUserInfo!);
    //    }
    //    return adUserList;
    //}

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
        // Note: We don't pass credentials to GetPrincipalContext here because ValidateCredentials
        // needs to use the default/config credentials to perform the validation against AD
        using PrincipalContext principalContext = GetPrincipalContext(ldapCredentials: null, searchBase: searchBase);
        var result = principalContext.ValidateCredentials(userName, password);
        if (result)
        {
            var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
            var adUserInfo = await FindUserAsync(userName, ldapCredentials, searchBase, cancellationToken).ConfigureAwait(false);
            return (true, adUserInfo);
        }
        return (false, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>xxx-DC.xxx.com</returns>
    public static string GetDomainController()
    {
        var directoryContext = new DirectoryContext(DirectoryContextType.Domain);
        string result;
        {
            var domainController = DomainController.FindOne(directoryContext);
            result = domainController.ToString();
        }

        return result;
    }

    public SearchResultCollection SearchUsersByFilter(string? filter, LdapCredentials? ldapCredentials = null, string? searchBase = null)
    {
        if (ldapCredentials == null)
        {
            if (ldapConfig.Credentials.IsNotNull() && ldapConfig.Credentials.BindDn.IsNotNullOrEmpty() && ldapConfig.Credentials.BindCredentials.IsNotNullOrEmpty())
            {
                ldapCredentials = ldapConfig.Credentials;
            }
        }

        using DirectoryEntry directoryEntry = CreateDirectoryEntry(ldapCredentials, searchBase);
        using DirectorySearcher directorySearcher = new(directoryEntry);
        directorySearcher.SearchScope = SearchScope.Subtree;
        directorySearcher.PageSize = 1000;

        if (ldapConfig.Attributes.IsNotNull())
        {
            // 加载默认属性
            foreach (var property in ldapConfig.Attributes)
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
        string? ldapPath;
        if (ldapConfig == null)
        {
            ldapPath = $"LDAP://{GetDomainController()}";
            return new DirectoryEntry(ldapPath);
        }
        else
        {
            // Use provided searchBase or fall back to config's SearchBase
            var effectiveSearchBase = searchBase ?? ldapConfig.SearchBase;
            ldapPath = $"LDAP://{ldapConfig.Url}/{effectiveSearchBase}";

            if (ldapCredentials == null)
            {
                return new DirectoryEntry(ldapPath);
            }

            var domain = ldapConfig.Domain;
            var userId = ldapCredentials.BindDn;
            var password = ldapCredentials.BindCredentials;

            return new DirectoryEntry(ldapPath, $@"{domain}\{userId}", password);
        }
    }

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    public async Task<bool> UserExistsAsync(string userName, string? searchBase = null, CancellationToken cancellationToken = default) => await FindUserAsync(userName, searchBase: searchBase, cancellationToken: cancellationToken).ConfigureAwait(false) != null;
}
