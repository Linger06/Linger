using System.Globalization;
using System.Text;

namespace Linger.Ldap.Contracts;

/// <summary>
/// Provider-agnostic helpers for building LDAP search filters and bind user names.
/// </summary>
public static class LdapHelper
{
    /// <summary>
    /// Builds a user search filter from the configured template, escaping the user input.
    /// </summary>
    /// <param name="userName">Raw user input (keyword or identity).</param>
    /// <param name="exactMatch">When false, a trailing wildcard is appended if none is present and user-supplied asterisks are preserved.</param>
    /// <param name="configuredTemplate">Optional configured filter template containing "{0}". Invalid templates fall back to <paramref name="defaultTemplate"/>.</param>
    /// <param name="defaultTemplate">Default filter template containing "{0}", used when no template is configured or the configured one is invalid.</param>
    /// <param name="usedFallback">True when the configured template was invalid and the default template was used instead.</param>
    /// <returns>The LDAP filter expression.</returns>
    public static string BuildUserSearchFilter(string userName, bool exactMatch, string? configuredTemplate, string defaultTemplate, out bool usedFallback)
    {
        usedFallback = false;

        var searchValue = userName;
        if (!exactMatch && searchValue.IndexOf('*') < 0)
        {
            searchValue += "*";
        }

        var escapedValue = EscapeFilterValue(searchValue, preserveAsterisk: !exactMatch);

        if (string.IsNullOrWhiteSpace(configuredTemplate))
        {
            return string.Format(CultureInfo.InvariantCulture, defaultTemplate, escapedValue);
        }

        if (configuredTemplate!.IndexOf("{0}", StringComparison.Ordinal) < 0)
        {
            usedFallback = true;
            return string.Format(CultureInfo.InvariantCulture, defaultTemplate, escapedValue);
        }

        try
        {
            return string.Format(CultureInfo.InvariantCulture, configuredTemplate, escapedValue);
        }
        catch (FormatException)
        {
            usedFallback = true;
            return string.Format(CultureInfo.InvariantCulture, defaultTemplate, escapedValue);
        }
    }

    /// <summary>
    /// Escapes special characters in an LDAP filter value per RFC 4515.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <param name="preserveAsterisk">When true, asterisks are kept as wildcards instead of being escaped.</param>
    public static string EscapeFilterValue(string value, bool preserveAsterisk = false)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escapedValue = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    escapedValue.Append("\\5c");
                    break;
                case '*':
                    escapedValue.Append(preserveAsterisk ? "*" : "\\2a");
                    break;
                case '(':
                    escapedValue.Append("\\28");
                    break;
                case ')':
                    escapedValue.Append("\\29");
                    break;
                case '\0':
                    escapedValue.Append("\\00");
                    break;
                default:
                    escapedValue.Append(character);
                    break;
            }
        }

        return escapedValue.ToString();
    }

    /// <summary>
    /// Builds a bind user name, prefixing the domain when the value is a plain account name.
    /// Values that already look like DOMAIN\user, a UPN, or a distinguished name are returned unchanged.
    /// </summary>
    /// <param name="bindDn">The configured bind DN or account name.</param>
    /// <param name="domain">The domain to prefix plain account names with.</param>
    public static string? BuildBindUserName(string? bindDn, string? domain)
    {
        if (string.IsNullOrWhiteSpace(bindDn))
        {
            return bindDn;
        }

#if NETSTANDARD2_0
        if (bindDn!.Contains("\\") || bindDn.Contains("@") || bindDn.Contains("="))
#else
        if (bindDn!.Contains('\\') || bindDn.Contains('@') || bindDn.Contains('='))
#endif
        {
            return bindDn;
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            return bindDn;
        }

        return $@"{domain}\{bindDn}";
    }

    /// <summary>
    /// Validates provider-independent LDAP configuration values.
    /// </summary>
    /// <param name="ldapConfig">The LDAP configuration.</param>
    public static void ValidateConfig(LdapConfig ldapConfig)
    {
#if NETSTANDARD2_0
        if (ldapConfig is null)
        {
            throw new ArgumentNullException(nameof(ldapConfig));
        }
#else
        ArgumentNullException.ThrowIfNull(ldapConfig);
#endif

        if (ldapConfig.MaxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ldapConfig), ldapConfig.MaxResults, "MaxResults must be greater than zero.");
        }

        ValidateCredentials(ldapConfig.Credentials);
    }

    /// <summary>
    /// Validates that bind credentials are either complete or omitted for anonymous binding.
    /// </summary>
    /// <param name="credentials">The credentials to validate.</param>
    public static void ValidateCredentials(LdapCredentials? credentials)
    {
        if (credentials is null)
        {
            return;
        }

        var hasBindDn = !string.IsNullOrWhiteSpace(credentials.BindDn);
        var hasPassword = !string.IsNullOrEmpty(credentials.BindCredentials);
        if (hasBindDn != hasPassword)
        {
            throw new ArgumentException("BindDn and BindCredentials must both be provided, or both omitted for anonymous binding.", nameof(credentials));
        }
    }
}
