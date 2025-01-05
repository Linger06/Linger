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
    /// <param name="userInfo">When successful, contains the user information</param>
    /// <returns>True if credentials are valid; otherwise, false</returns>
    bool ValidateUser(string userName, string password, out AdUserInfo? userInfo);

    /// <summary>
    /// Finds a user in LDAP directory
    /// </summary>
    /// <param name="userName">Username to find</param>
    /// <param name="credentials">Optional LDAP credentials for binding</param>
    /// <returns>User information if found; otherwise, null</returns>
    AdUserInfo? FindUser(string userName, LdapCredentials? credentials = null);

    /// <summary>
    /// Gets all users matching the specified filter
    /// </summary>
    /// <param name="filter">LDAP filter string</param>
    /// <param name="credentials">Optional LDAP credentials for binding</param>
    /// <returns>Collection of matching users; null if none found</returns>
    IEnumerable<AdUserInfo>? GetUsers(string filter, LdapCredentials? credentials = null);
}