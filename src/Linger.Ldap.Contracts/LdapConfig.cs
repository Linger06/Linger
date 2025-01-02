namespace Linger.Ldap.Contracts;

/// <summary>
/// LDAP configuration class
/// </summary>
public class LdapConfig
{
    /// <summary>
    /// Hostname
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Whether to enable secure connection
    /// </summary>
    public bool Security { get; set; }

    /// <summary>
    /// Domain name
    /// </summary>
    public string Domain { get; set; }= null!;

    /// <summary>
    /// LDAP credentials
    /// </summary>
    public LdapCredentials? Credentials { get; set; }

    /// <summary>
    /// Search base, e.g., "DC=AC,DC=LOCAL"
    /// </summary>
    public string SearchBase { get; set; }= null!;

    /// <summary>
    /// Search filter, e.g., "(&(objectClass=user)(objectClass=person)(sAMAccountName={0}))"
    /// </summary>
    public string SearchFilter { get; set; }= null!;

    /// <summary>
    /// Attributes to retrieve, e.g., ["memberOf", "displayName", "sAMAccountName", "userPrincipalName"]
    /// </summary>
    public string[]? Attributes { get; set; }
}

