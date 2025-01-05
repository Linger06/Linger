using Linger.Ldap.Contracts;

namespace Linger.Ldap.AspNetCore;

public class LdapService : ILdapService
{
    private readonly ILdap _ldap;

    public LdapService(ILdap ldap)
    {
        _ldap = ldap ?? throw new ArgumentNullException(nameof(ldap));
    }

    public bool ValidateUser(string userName, string password, out AdUserInfo? userInfo)
        => _ldap.ValidateUser(userName, password, out userInfo);

    public async Task<(bool IsValid, AdUserInfo? UserInfo)> ValidateUserAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var isValid = _ldap.ValidateUser(userName, password, out AdUserInfo? userInfo);
        return await Task.FromResult((isValid, userInfo));
    }

    public async Task<AdUserInfo?> FindUserAsync(
        string userName,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default)
        => await Task.FromResult(_ldap.FindUser(userName, credentials));

    public async Task<IEnumerable<AdUserInfo>?> GetUsersAsync(
        string filter,
        LdapCredentials? credentials = null,
        CancellationToken cancellationToken = default)
        => await Task.FromResult(_ldap.GetUsers(filter, credentials));

    public async Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken = default)
        => await FindUserAsync(userName, cancellationToken: cancellationToken) != null;
}
