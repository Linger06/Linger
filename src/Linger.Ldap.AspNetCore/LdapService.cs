using Linger.Ldap.Contracts;

namespace Linger.Ldap.AspNetCore;

public class LdapService(ILdap ldap) : ILdapService
{
    private readonly ILdap _ldap = ldap ?? throw new ArgumentNullException(nameof(ldap));

    public async Task<(bool IsValid, AdUserInfo? UserInfo)> ValidateUserAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await _ldap.ValidateUserAsync(userName, password);
    }

    public async Task<AdUserInfo?> FindUserAsync(
        string userName,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default)
        => await _ldap.FindUserAsync(userName, credentials);

    public async Task<IEnumerable<AdUserInfo>> GetUsersAsync(
        string userName,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default)
        => await _ldap.GetUsersAsync(userName, credentials);

    public async Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken = default)
        => await FindUserAsync(userName, cancellationToken: cancellationToken) != null;
}
