namespace Linger.Ldap.Contracts;

/// <summary>
/// Represents asynchronous LDAP operations backed by a provider with native asynchronous APIs.
/// </summary>
/// <remarks>
/// The Active Directory provider uses a separate synchronous contract because
/// <c>System.DirectoryServices</c> does not provide asynchronous search operations.
/// </remarks>
public interface ILdapClient
{
    /// <summary>
    /// Validates user credentials against LDAP directory
    /// </summary>
    /// <param name="userName">Username to validate</param>
    /// <param name="password">Password to validate</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if credentials are valid; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is blank or <paramref name="password"/> is empty.</exception>
    Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateUserAsync(string userName, string password, string? searchBase = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user in LDAP directory
    /// </summary>
    /// <param name="userName">Username to find</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if found; otherwise, null</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is blank.</exception>
    Task<LdapUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by keyword or identity.
    /// </summary>
    /// <param name="userName">Keyword or identity to search (e.g., account, UPN, email, display name)</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching users</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is blank.</exception>
    Task<IReadOnlyList<LdapUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users by a raw LDAP filter.
    /// </summary>
    /// <param name="filter">Raw LDAP filter expression</param>
    /// <param name="ldapCredentials">Optional LDAP credentials for binding</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching users</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filter"/> is blank.</exception>
    Task<IReadOnlyList<LdapUserInfo>> SearchUsersByFilterAsync(string filter, LdapCredentials? ldapCredentials = null, string? searchBase = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="searchBase">Optional specific OU to search in. If null, uses default from config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is blank.</exception>
    Task<bool> UserExistsAsync(string userName, string? searchBase = null, CancellationToken cancellationToken = default);
}
