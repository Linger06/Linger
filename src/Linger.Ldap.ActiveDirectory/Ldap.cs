using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;
using Linger.Extensions.Core;
using Linger.Helper;
using Linger.Ldap.Contracts;

namespace Linger.Ldap.ActiveDirectory;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class Ldap(LdapConfig ldapConfig) : ILdap
{
    private const string BaseFilter = "(&(objectCategory=person)(objectClass=user))";

    /// <summary>
    /// Gets a certain user on Active Directory
    /// </summary>
    /// <param name="userName">The username to get</param>
    /// <param name="ldapCredentials"></param>
    /// <returns>Returns the UserPrincipal Object</returns>
    public AdUserInfo? FindUser(string userName, LdapCredentials? ldapCredentials = null)
    {
        PrincipalContext principalContext = GetPrincipalContext(ldapCredentials);
        UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, userName);
        return userPrincipal.ToAdUser();
    }

    public DirectoryEntry GetEntryByUsername(string username)
    {
        PrincipalContext ctx = new PrincipalContext(ContextType.Domain, ldapConfig.Url);
        UserPrincipal user = UserPrincipal.FindByIdentity(ctx, username);
        var entry = user.GetUnderlyingObject() as DirectoryEntry;
        entry.EnsureIsNotNull();
        return entry;
    }

    /// <summary>
    /// Gets the base principal context
    /// </summary>
    /// <returns>Returns the PrincipalContext object</returns>
    private PrincipalContext GetPrincipalContext(LdapCredentials? ldapCredentials = null)
    {
        if (ldapCredentials == null)
        {
            if (ldapConfig.Credentials.IsNotNull() && ldapConfig.Credentials.BindDn.IsNotNullAndEmpty() && ldapConfig.Credentials.BindCredentials.IsNotNullAndEmpty())
            {
                ldapCredentials = ldapConfig.Credentials;
            }
        }

        if (ldapCredentials == null)
        {
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ldapConfig.Url, ldapConfig.SearchBase, ContextOptions.SimpleBind);
            return principalContext;
        }
        else
        {
            var domain = ldapConfig.Domain;
            var userId = ldapCredentials.BindDn;
            var password = ldapCredentials.BindCredentials;
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ldapConfig.Url, ldapConfig.SearchBase, ContextOptions.SimpleBind,
            $@"{domain}\{userId}", password);
            return principalContext;
        }
    }

    public IEnumerable<AdUserInfo> GetUsers(string userName, LdapCredentials? ldapCredentials = null)
    {
        var collection = SearchUsersByFilter($"""(samAccountName={userName}*)(userPrincipalName={userName}*)(mail={userName}*)(displayName={userName}*)""", ldapCredentials);
        return collection.ToAdUsersInfo();
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
    /// <param name="adUserInfo"></param>
    /// <returns>Returns True of user is valid</returns>
    public bool ValidateUser(string userName, string password, out AdUserInfo? adUserInfo)
    {
        PrincipalContext principalContext = GetPrincipalContext();
        var result = principalContext.ValidateCredentials(userName, password);
        if (result)
        {
            var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
            adUserInfo = FindUser(userName, ldapCredentials);
            return true;
        }
        else
        {
            adUserInfo = null;
            return false;
        }
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

    public SearchResultCollection SearchUsersByFilter(string? filter, LdapCredentials? ldapCredentials = null)
    {
        if (ldapCredentials == null)
        {
            if (ldapConfig.Credentials.IsNotNull() && ldapConfig.Credentials.BindDn.IsNotNullAndEmpty() && ldapConfig.Credentials.BindCredentials.IsNotNullAndEmpty())
            {
                ldapCredentials = ldapConfig.Credentials;
            }
        }

        using DirectoryEntry directoryEntry = CreateDirectoryEntry(ldapCredentials);
        using DirectorySearcher directorySearcher = new(directoryEntry)
        {
            SearchScope = SearchScope.Subtree,
            PageSize = 1000
        };

        if (ldapConfig.Attributes.IsNotNull())
        {
            // 加载默认属性
            foreach (var property in ldapConfig.Attributes)
            {
                directorySearcher.PropertiesToLoad.Add(property);
            }
        }

        // 构建搜索过滤器
        directorySearcher.Filter = string.IsNullOrEmpty(filter)
            ? BaseFilter
            : $"{BaseFilter}(|{filter})";

        return directorySearcher.FindAll();
    }

    private DirectoryEntry CreateDirectoryEntry(LdapCredentials? ldapCredentials)
    {
        var ldapPath = $"LDAP://{ldapConfig.Url}/{ldapConfig.SearchBase}";

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