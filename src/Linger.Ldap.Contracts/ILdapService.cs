namespace Linger.Ldap.Contracts;

/// <summary>
/// Provides high-level LDAP directory service operations
/// </summary>
public interface ILdapService
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
    /// Validates user credentials asynchronously
    /// </summary>
    /// <param name="userName">Username to validate</param>
    /// <param name="password">Password to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if validation succeeds; otherwise, null</returns>
    Task<(bool IsValid, AdUserInfo? UserInfo)> ValidateUserAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user in LDAP directory
    /// </summary>
    /// <param name="userName">Username to find</param>
    /// <param name="credentials">Optional LDAP credentials for binding</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if found; otherwise, null</returns>
    Task<AdUserInfo?> FindUserAsync(
        string userName,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AdUserInfo>?> GetUsersAsync(
        string userName,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken = default);
}