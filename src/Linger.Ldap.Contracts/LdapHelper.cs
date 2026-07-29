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
    /// <param name="configuredTemplate">Optional configured filter template containing "{0}". A template without "{0}" is returned as-is.</param>
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
            return configuredTemplate;
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
}
