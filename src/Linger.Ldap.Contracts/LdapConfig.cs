namespace Linger.Ldap.Contracts;

/// <summary>
/// LDAP configuration class
/// </summary>
public class LdapConfig
{
    /// <summary>
    /// Initializes an empty LDAP configuration.
    /// </summary>
    public LdapConfig()
    {
    }

    /// <summary>
    /// Initializes an independent copy of an LDAP configuration.
    /// </summary>
    /// <param name="source">The configuration to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <example>
    /// <code>var snapshot = new LdapConfig(configuration);</code>
    /// </example>
    public LdapConfig(LdapConfig source)
    {
#if NETSTANDARD2_0
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
#else
        ArgumentNullException.ThrowIfNull(source);
#endif

        Url = source.Url;
        Security = source.Security;
        Domain = source.Domain;
        SearchBase = source.SearchBase;
        SearchFilter = source.SearchFilter;
        MaxResults = source.MaxResults;
        Attributes = source.Attributes is null ? null : (string[])source.Attributes.Clone();
        Credentials = source.Credentials is null
            ? null
            : new LdapCredentials
            {
                BindDn = source.Credentials.BindDn,
                BindCredentials = source.Credentials.BindCredentials
            };
    }

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
    public string Domain { get; set; } = null!;

    /// <summary>
    /// LDAP credentials
    /// </summary>
    public LdapCredentials? Credentials { get; set; }

    /// <summary>
    /// Search base, e.g., "DC=AC,DC=LOCAL"
    /// </summary>
    public string SearchBase { get; set; } = null!;

    /// <summary>
    /// Search filter, e.g., "(&(objectClass=user)(objectClass=person)(sAMAccountName={0}))"
    /// </summary>
    public string SearchFilter { get; set; } = null!;

    /// <summary>
    /// Attributes to retrieve, e.g., ["memberOf", "displayName", "sAMAccountName", "userPrincipalName"]
    /// </summary>
    public string[]? Attributes { get; set; }

    /// <summary>
    /// Maximum number of users returned by one query.
    /// </summary>
    public int MaxResults { get; set; } = 1000;
}

