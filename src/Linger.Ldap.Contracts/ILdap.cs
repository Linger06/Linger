namespace Linger.Ldap.Contracts;


/// <summary>
/// Represents low-level LDAP operations
/// </summary>
public interface ILdap
{
    /// <summary>
    /// Validates user credentials against LDAP directory
    /// </summary>
    /// <param name="userName">Username to validate</param>
    /// <param name="password">Password to validate</param>
    /// <returns>True if credentials are valid; otherwise, false</returns>
    Task<(bool IsValid, AdUserInfo? AdUserInfo)> ValidateUserAsync(string userName, string password);

    /// <summary>
    /// Finds a user in LDAP directory
    /// </summary>
    /// <param name="userName">Username to find</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <returns>User information if found; otherwise, null</returns>
    Task<AdUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null);

    /// <summary>
    /// Gets all users matching the specified filter
    /// </summary>
    /// <param name="userName">LDAP filter string</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <returns>Collection of matching users; null if none found</returns>
    Task<IEnumerable<AdUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null);
}