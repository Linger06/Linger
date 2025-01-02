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

    public AdUserInfo? FindUser(string userName, LdapCredentials? ldapCredentials = null)
    {
        if (!Connect(ldapCredentials)) return null;

        try
        {
            var searchFilter = string.Format(_ldapConfig.SearchFilter, userName);
            ILdapSearchResults? result = _ldapConn.Search(_ldapConfig.SearchBase, LdapConnection.ScopeSub, searchFilter, null, false);

            LdapEntry? user = result.Next();
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

    public IEnumerable<AdUserInfo>? GetUsers(string userName, LdapCredentials? ldapCredentials = null)
    {
        if (!Connect(ldapCredentials)) return null;

        try
        {
            var ldapEntries = new List<LdapEntry>();

            var searchFilter = string.Format(_ldapConfig.SearchFilter, userName + "*");
            ILdapSearchResults? lsc = _ldapConn.Search(_ldapConfig.SearchBase, LdapConnection.ScopeSub, searchFilter, null, false);
            while (lsc.HasMore())
            {
                LdapEntry? nextEntry;
                try
                {
                    nextEntry = lsc.Next();
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

    public bool ValidateUser(string userName, string password, out AdUserInfo? adUserInfo)
    {
        var ldapCredentials = new LdapCredentials { BindDn = userName, BindCredentials = password };
        adUserInfo = FindUser(userName, ldapCredentials);

        if (adUserInfo == null) return false;

        try
        {
            if (Connect())
            {
                _ldapConn.Bind(adUserInfo.Dn, password);
                return _ldapConn.Bound;
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

        return false;
    }

    public bool IsConnected() => _ldapConn.Connected;

    public bool Connect(LdapCredentials? ldapCredentials = null)
    {
        if (_ldapConn.Connected) return true;

        try
        {
            _ldapConn.Connect(_ldapConfig.Url, _ldapConfig.Security ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort);

            if (ldapCredentials != null)
            {
                BindCredentials(ldapCredentials);
            }
            else if (_ldapConfig.Credentials != null)
            {
                BindCredentials(_ldapConfig.Credentials);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void BindCredentials(LdapCredentials credentials)
    {
        var domain = _ldapConfig.Domain;
        var userId = credentials.BindDn;
        var password = credentials.BindCredentials;

        if (userId.IsNotNullAndEmpty() && password.IsNotNullAndEmpty())
        {
            _ldapConn.Bind($@"{domain}\{userId}", password);
        }
        else
        {
            _ldapConn.Bind(null, null);
        }
    }

    public void DisConnect()
    {
        if (_ldapConn.Connected)
        {
            _ldapConn.Disconnect();
        }
    }
}