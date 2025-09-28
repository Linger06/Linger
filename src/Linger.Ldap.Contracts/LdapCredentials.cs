namespace Linger.Ldap.Contracts;

/// <summary>
/// LDAP credentials class
/// </summary>
public class LdapCredentials
{
    /// <summary>
    /// Bind distinguished name
    /// </summary>
    public string? BindDn { get; set; }

    /// <summary>
    /// Bind credentials (password)
    /// </summary>
    public string? BindCredentials { get; set; }
}
