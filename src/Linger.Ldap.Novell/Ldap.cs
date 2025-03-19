using Linger.Extensions.Core;
using Linger.Ldap.Contracts;
using Novell.Directory.Ldap;

namespace Linger.Ldap.Novell;

public class Ldap : ILdap
{
    private readonly LdapConfig _ldapConfig;
    private readonly LdapConnection _ldapConn;

    public Ldap(LdapConfig ldapConfig)
    {
        _ldapConfig = ldapConfig;
        _ldapConn = new LdapConnection { SecureSocketLayer = _ldapConfig.Security };
    }

    public async Task<AdUserInfo?> FindUserAsync(string userName, LdapCredentials? ldapCredentials = null)
    {
        if (!await ConnectAsync(ldapCredentials)) return null;

        try
        {
            var searchFilter = string.Format(_ldapConfig.SearchFilter, userName);
            ILdapSearchResults? result = await _ldapConn.SearchAsync(_ldapConfig.SearchBase, LdapConnection.ScopeSub, searchFilter, null, false);

            LdapEntry? user = await result.NextAsync();
            return user?.ToAdUser();
        }
        catch (Exception ex)
        {
            throw new Exception("Login failed.", ex);
        }
        finally
        {
            DisConnect();
        }
    }

    public async Task<IEnumerable<AdUserInfo>> GetUsersAsync(string userName, LdapCredentials? ldapCredentials = null)
    {
        if (!await ConnectAsync(ldapCredentials)) return [];

        try
        {
            var ldapEntries = new List<LdapEntry>();

            var searchFilter = string.Format(_ldapConfig.SearchFilter, userName + "*");
            ILdapSearchResults? lsc = await _ldapConn.SearchAsync(_ldapConfig.SearchBase, LdapConnection.ScopeSub, searchFilter, null, false);
            while (await lsc.HasMoreAsync())
            {
                LdapEntry? nextEntry;
                try
                {
                    nextEntry = await lsc.NextAsync();
                }
                catch (LdapException e)
                {
                    Console.WriteLine("Error: " + e.LdapErrorMessage);
                    //Exception is thrown, go for next entry
                    continue;
                }
                ldapEntries.Add(nextEntry);
            }
            var adUserInfos = new List<AdUserInfo>();
            if (ldapEntries.Count > 0)
            {
                ldapEntries.ForEach(ldapEntry =>
                {
                    AdUserInfo? adUser = ldapEntry.ToAdUser();
                    if (adUser != null)
                    {
                        adUserInfos.Add(adUser);
                    }
                });
            }
            return adUserInfos;
        }
        catch (Exception ex)
        {
            throw new Exception("Login failed.", ex);
        }
        finally
        {
            DisConnect();
        }
    }

    public async Task<(bool IsValid, AdUserInfo? AdUserInfo)> ValidateUserAsync(string userName, string password)
    {
        var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
        var adUserInfo = await FindUserAsync(userName, ldapCredentials);

        if (adUserInfo == null) return (false, null);

        try
        {
            if (await ConnectAsync())
            {
                await _ldapConn.BindAsync(adUserInfo.Dn, password);
                return (_ldapConn.Bound, adUserInfo);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Login failed.", ex);
        }
        finally
        {
            DisConnect();
        }

        return (false, null);
    }

    public bool IsConnected() => _ldapConn.Connected;

    public async Task<bool> ConnectAsync(LdapCredentials? ldapCredentials = null)
    {
        if (_ldapConn.Connected) return true;

        try
        {
            await _ldapConn.ConnectAsync(_ldapConfig.Url, _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort);

            if (ldapCredentials != null)
            {
                await BindCredentialsAsync(ldapCredentials);
            }
            else if (_ldapConfig.Credentials != null)
            {
                await BindCredentialsAsync(_ldapConfig.Credentials);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task BindCredentialsAsync(LdapCredentials credentials)
    {
        var domain = _ldapConfig.Domain;
        var userId = credentials.BindDn;
        var password = credentials.BindCredentials;

        if (userId.IsNotNullAndEmpty() && password.IsNotNullAndEmpty())
        {
            await _ldapConn.BindAsync($@"{domain}\{userId}", password);
        }
        else
        {
            await _ldapConn.BindAsync(null, null);
        }
    }

    public void DisConnect()
    {
        if (_ldapConn.Connected)
        {
            _ldapConn.Disconnect();
        }
    }

    /// <summary>
    /// Checks if user exists in LDAP directory
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists; otherwise, false</returns>
    public async Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken = default) => await FindUserAsync(userName) != null;

}