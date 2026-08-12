namespace Linger.Ldap.Contracts;

/// <summary>
/// Defines synchronous operations backed by the Windows Active Directory APIs.
/// </summary>
public interface IActiveDirectoryClient
{
    /// <summary>
    /// Validates user credentials against Active Directory.
    /// </summary>
    /// <param name="userName">Username to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <param name="searchBase">Optional specific OU to search in.</param>
    /// <returns>True and user information when credentials are valid; otherwise false and null.</returns>
    (bool IsValid, LdapUserInfo? LdapUserInfo) ValidateUser(
        string userName,
        string password,
        string? searchBase = null);

    /// <summary>
    /// Finds a user in Active Directory.
    /// </summary>
    /// <param name="userName">Username to find.</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding.</param>
    /// <param name="searchBase">Optional specific OU to search in.</param>
    /// <returns>User information if found; otherwise null.</returns>
    LdapUserInfo? FindUser(
        string userName,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null);

    /// <summary>
    /// Gets users by keyword or identity.
    /// </summary>
    /// <param name="userName">Keyword or identity to search.</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding.</param>
    /// <param name="searchBase">Optional specific OU to search in.</param>
    /// <returns>Collection of matching users.</returns>
    IReadOnlyList<LdapUserInfo> GetUsers(
        string userName,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null);

    /// <summary>
    /// Searches users by a raw LDAP filter.
    /// </summary>
    /// <param name="filter">Raw LDAP filter expression.</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding.</param>
    /// <param name="searchBase">Optional specific OU to search in.</param>
    /// <returns>Collection of matching users.</returns>
    IReadOnlyList<LdapUserInfo> SearchUsersByFilter(
        string filter,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null);

    /// <summary>
    /// Checks if a user exists in Active Directory.
    /// </summary>
    /// <param name="userName">Username to check.</param>
    /// <param name="searchBase">Optional specific OU to search in.</param>
    /// <returns>True if the user exists; otherwise false.</returns>
    bool UserExists(string userName, string? searchBase = null);
}
